using System;
using System.Threading.Tasks;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Bil2.Contract.BlocksReader.Commands;
using Lykke.Common.Chaos;
using Lykke.Job.Bil2Indexer.Contract.Events;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;

namespace Lykke.Job.Bil2Indexer.DomainServices
{
    public class ChainCrawler : IChainCrawler
    {
        private readonly string _blockchainType;
        
        /// <summary>
        /// Inclusive block number to start crawling from.
        /// </summary>
        private readonly long _startBlock;
        
        /// <summary>
        /// Exclusive block number to stop crawling on, or null to continue crawling forever.
        /// </summary>
        private readonly long? _stopBlock;
        
        private readonly IChaosKitty _chaosKitty;
        private readonly IContractEventsPublisher _contractEventsPublisher;
        private readonly IBlocksReaderApi _blocksReaderApi;
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly IBlockExpectationRepository _blockExpectationRepository;
        private readonly IBlocksDeduplicationRepository _blocksDeduplicationRepository;
        private readonly string _id;

        public ChainCrawler(
            string blockchainType,
            long startBlock,
            long? stopBlock,
            IChaosKitty chaosKitty,
            IContractEventsPublisher contractEventsPublisher,
            IBlocksReaderApi blocksReaderApi,
            IBlockHeadersRepository blockHeadersRepository,
            IBlockExpectationRepository blockExpectationRepository,
            IBlocksDeduplicationRepository blocksDeduplicationRepository)
        {
            _blockchainType = blockchainType;
            _startBlock = startBlock;
            _stopBlock = stopBlock;
            _chaosKitty = chaosKitty;
            _contractEventsPublisher = contractEventsPublisher;
            _blocksReaderApi = blocksReaderApi;
            _blockHeadersRepository = blockHeadersRepository;
            _blockExpectationRepository = blockExpectationRepository;
            _blocksDeduplicationRepository = blocksDeduplicationRepository;

            _id = $"{startBlock}-{stopBlock}";
        }

        public async Task StartAsync()
        {
            // If no block was expected, then start from the scratch
            var blockExpectation = await _blockExpectationRepository.GetOrDefaultAsync(_id);

            if (blockExpectation == null)
            {
                blockExpectation = new BlockExpectation(_startBlock);
                await _blockExpectationRepository.SaveAsync(_id, blockExpectation);
            }
            
            await _blocksReaderApi.SendAsync(new ReadBlockCommand(blockExpectation.Number));
        }

        public async Task ProcessBlockAsync(BlockHeader block)
        {
            if (block.Number < _startBlock || _stopBlock.HasValue && block.Number >= _stopBlock.Value)
            {
                return;
            }

            // TODO: Have to:
            // 1. read state, make a decision, send command
            // 2. update state in the command handler, publish event

            if (await _blocksDeduplicationRepository.IsProcessedAsync(block.Hash))
            {
                return;
            }

            var (previousBlock, blockExpectation) = await TaskExecution.WhenAll
            (
                _blockHeadersRepository.GetOrDefaultAsync(block.Number - 1),
                _blockExpectationRepository.GetOrDefaultAsync(_id)
            );

            if (blockExpectation != null && block.Number != blockExpectation.Number)
            {
                throw new InvalidOperationException($"Disordered block: [{block.Number}], expected block: [{blockExpectation.Number}]");
            }
            
            var nextBlockExpectation = previousBlock == null || block.PreviousBlockHash == previousBlock.Hash
                ? await MoveForwardAsync(blockExpectation, block)
                : await MoveBackwardAsync(blockExpectation, block, previousBlock);

            if (!_stopBlock.HasValue || nextBlockExpectation.Number < _stopBlock.Value)
            {
                await _blocksReaderApi.SendAsync(new ReadBlockCommand(nextBlockExpectation.Number));
            }

            _chaosKitty.Meow(block.Hash);

            await _blocksDeduplicationRepository.MarkAsProcessedAsync(block.Hash);
        }

        private async Task<BlockExpectation> MoveForwardAsync(BlockExpectation blockExpectation, BlockHeader block)
        {          
            var skippedBlocksNumber = await SkipAlreadyReadBlocks(block);
            var nextBlockExpectation = blockExpectation.Skip(skippedBlocksNumber);

            await Task.WhenAll
            (
                _blockHeadersRepository.SaveAsync(block),
                _blockExpectationRepository.SaveAsync(_id, nextBlockExpectation)
            );

            _chaosKitty.Meow(block.Hash);

            return nextBlockExpectation;
        }

        private async Task<long> SkipAlreadyReadBlocks(BlockHeader block)
        {
            var nextBlockNumber = block.Number;
            var currentBlock = block;

            while (true)
            {
                nextBlockNumber++;

                if (_stopBlock.HasValue && nextBlockNumber >= _stopBlock)
                {
                    break;
                }

                var storedNextBlock = await _blockHeadersRepository.GetOrDefaultAsync(nextBlockNumber);

                if (storedNextBlock == null)
                {
                    break;
                }

                // Removes already stored blocks, which belongs to another chain.
                // For example, if chain was switched during the backward turn, thus
                // already read on the backward turn blocks are belongs to the stale chain.

                if (storedNextBlock.PreviousBlockHash != currentBlock.Hash)
                {
                    await Task.WhenAll
                    (
                        _blockHeadersRepository.RemoveAsync(storedNextBlock),
                        _blocksDeduplicationRepository.MarkAsNotProcessedAsync(storedNextBlock.Hash)
                    );

                    _chaosKitty.Meow(block.Hash);

                    await _contractEventsPublisher.PublishAsync(new BlockRolledBackEvent
                    {
                        BlockchainType = _blockchainType,
                        BlockNumber = storedNextBlock.Number,
                        BlockHash = storedNextBlock.Hash,
                        PreviousBlockHash = storedNextBlock.PreviousBlockHash
                    });

                    break;
                }

                currentBlock = storedNextBlock;
            }

            return nextBlockNumber - block.Number;
        }

        private async Task<BlockExpectation> MoveBackwardAsync(BlockExpectation blockExpectation, BlockHeader block, BlockHeader previousBlock)
        {
            // TODO: This is not idempotent. Should be processed in independent command handler
            var nextBlockToRead = blockExpectation.Previous();

            var removePreviousBlockTask = _blockHeadersRepository.RemoveAsync(previousBlock);
            var markPreviousBlockAsNotProcessedTask = _blocksDeduplicationRepository.MarkAsNotProcessedAsync(previousBlock.Hash);
            var saveBlockTask = _blockHeadersRepository.SaveAsync(block);

            await Task.WhenAll
            (
                removePreviousBlockTask,
                markPreviousBlockAsNotProcessedTask,
                saveBlockTask,
                _blockExpectationRepository.SaveAsync(_id, nextBlockToRead)
            );

            _chaosKitty.Meow(block.Hash);

            await _contractEventsPublisher.PublishAsync(new BlockRolledBackEvent
            {
                BlockchainType = _blockchainType,
                BlockNumber = previousBlock.Number,
                BlockHash = previousBlock.Hash,
                PreviousBlockHash = previousBlock.PreviousBlockHash
            });

            return nextBlockToRead;
        }
    }
}
