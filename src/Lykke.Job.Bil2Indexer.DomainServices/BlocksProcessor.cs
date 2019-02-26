using System;
using System.Threading.Tasks;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Bil2.Contract.BlocksReader.Commands;
using Lykke.Job.Bil2Indexer.Contract.Events;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;

namespace Lykke.Job.Bil2Indexer.DomainServices
{
    public class BlocksProcessor : IBlocksProcessor
    {
        private readonly string _blockchainType;
        private readonly int _startBlock;
        private readonly IContractEventsPublisher _contractEventsPublisher;
        private readonly IBlocksReaderApi _blocksReaderApi;
        private readonly IBlocksRepository _blocksRepository;
        private readonly IBlockExpectationRepository _blockExpectationRepository;
        private readonly IBlocksDeduplicationRepository _blocksDeduplicationRepository;

        public BlocksProcessor(
            string blockchainType,
            int startBlock,
            IContractEventsPublisher contractEventsPublisher,
            IBlocksReaderApi blocksReaderApi,
            IBlocksRepository blocksRepository,
            IBlockExpectationRepository blockExpectationRepository,
            IBlocksDeduplicationRepository blocksDeduplicationRepository)
        {
            _blockchainType = blockchainType;
            _startBlock = startBlock;
            _contractEventsPublisher = contractEventsPublisher;
            _blocksReaderApi = blocksReaderApi;
            _blocksRepository = blocksRepository;
            _blockExpectationRepository = blockExpectationRepository;
            _blocksDeduplicationRepository = blocksDeduplicationRepository;
        }

        public async Task StartAsync()
        {
            // If no block was expected, then start from the scratch
            var blockExpectation = await _blockExpectationRepository.GetOrDefaultAsync() ??
                                new BlockExpectation(_startBlock);

            await _blockExpectationRepository.SaveAsync(blockExpectation);
            await _blocksReaderApi.SendAsync(new ReadBlockCommand(blockExpectation.Number));
        }

        public async Task ProcessBlockAsync(BlockHeader block)
        {
            if (await _blocksDeduplicationRepository.IsProcessedAsync(block.Hash))
            {
                return;
            }

            var (previousBlock, blockExpectation) = await TaskExecution.WhenAll
            (
                _blocksRepository.GetOrDefaultAsync(block.Number - 1),
                _blockExpectationRepository.GetOrDefaultAsync()
            );

            if (blockExpectation != null && block.Number != blockExpectation.Number)
            {
                throw new InvalidOperationException($"Disordered block: [{block.Number}], expected block: [{blockExpectation.Number}]");
            }
            
            var nextBlockExpectation = previousBlock == null || block.PreviousBlockHash == previousBlock.Hash
                ? await MoveForwardAsync(blockExpectation, block)
                : await MoveBackwardAsync(blockExpectation, block, previousBlock);

            await _blocksReaderApi.SendAsync(new ReadBlockCommand(nextBlockExpectation.Number));
            await _blocksDeduplicationRepository.MarkAsProcessedAsync(block.Hash);
        }

        private async Task<BlockExpectation> MoveForwardAsync(BlockExpectation blockExpectation, BlockHeader block)
        {          
            var skippedBlocksNumber = await SkipAlreadyReadBlocks(block);
            var nextBlockExpectation = blockExpectation.Skip(skippedBlocksNumber);

            await Task.WhenAll
            (
                _blocksRepository.SaveAsync(block),
                _blockExpectationRepository.SaveAsync(nextBlockExpectation)
            );

            return nextBlockExpectation;
        }

        private async Task<long> SkipAlreadyReadBlocks(BlockHeader block)
        {
            var nextBlockNumber = block.Number;
            var currentBlock = block;

            while (true)
            {
                nextBlockNumber++;

                var storedNextBlock = await _blocksRepository.GetOrDefaultAsync(nextBlockNumber);

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
                        _blocksRepository.RemoveAsync(storedNextBlock),
                        _blocksDeduplicationRepository.MarkAsNotProcessedAsync(storedNextBlock.Hash)
                    );
                    break;
                }

                currentBlock = storedNextBlock;
            }

            return nextBlockNumber - block.Number;
        }

        private async Task<BlockExpectation> MoveBackwardAsync(BlockExpectation blockExpectation, BlockHeader block, BlockHeader previousBlock)
        {
            var nextBlockToRead = blockExpectation.Previous();

            var removePreviousBlockTask = _blocksRepository.RemoveAsync(previousBlock);
            var markPreviousBlockAsNotProcessedTask = _blocksDeduplicationRepository.MarkAsNotProcessedAsync(previousBlock.Hash);
            var saveBlockTask = _blocksRepository.SaveAsync(block);

            await Task.WhenAll
            (
                removePreviousBlockTask,
                markPreviousBlockAsNotProcessedTask,
                saveBlockTask,
                _blockExpectationRepository.SaveAsync(nextBlockToRead)
            );

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
