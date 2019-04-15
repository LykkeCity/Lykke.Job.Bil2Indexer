//using System;
//using System.Threading.Tasks;
//using Lykke.Bil2.Client.BlocksReader;
//using Lykke.Bil2.Client.BlocksReader.Services;
//using Lykke.Bil2.Contract.BlocksReader.Commands;
//using Lykke.Bil2.RabbitMq.Publication;
//using Lykke.Common.Chaos;
//using Lykke.Job.Bil2Indexer.Contract.Events;
//using Lykke.Job.Bil2Indexer.Domain;
//using Lykke.Job.Bil2Indexer.Domain.Repositories;
//using Lykke.Job.Bil2Indexer.Domain.Services;

//namespace Lykke.Job.Bil2Indexer.DomainServices
//{
//    // TODO: Нужен дедупликатор всех евентов.
//    // TODO: При удалении блока, удалять все его транзакции и AccountActions.
//    // TODO: Нужна пошаговая обработка блока - собрали, обновили actions.
//    // TODO: В blockHeadersRepository нужен blockchainType

//    public class ChainCrawler : IChainCrawler
//    {
//        public string Id { get; private set; }

//        private readonly string _blockchainType;
        
//        /// <summary>
//        /// Inclusive block number to start crawling from.
//        /// </summary>
//        private readonly long _startBlock;
        
//        /// <summary>
//        /// Exclusive block number to stop crawling on, or null to continue crawling forever.
//        /// </summary>
//        private readonly long? _stopBlock;
        
//        private readonly IChaosKitty _chaosKitty;
//        private readonly IBlocksReaderApi _blocksReaderApi;
//        private readonly IBlockHeadersRepository _blockHeadersRepository;
//        private readonly ICrawlersRepository _crawlersRepository;
//        private readonly IBlocksDeduplicationRepository _blocksDeduplicationRepository;

//        public ChainCrawler(
//            string blockchainType,
//            long startBlock,
//            long? stopBlock,
//            IChaosKitty chaosKitty,
//            IBlocksReaderApi blocksReaderApi,
//            IBlockHeadersRepository blockHeadersRepository,
//            ICrawlersRepository crawlersRepository,
//            IBlocksDeduplicationRepository blocksDeduplicationRepository)
//        {
//            _blockchainType = blockchainType;
//            _startBlock = startBlock;
//            _stopBlock = stopBlock;
//            _chaosKitty = chaosKitty;
//            _blocksReaderApi = blocksReaderApi;
//            _blockHeadersRepository = blockHeadersRepository;
//            _crawlersRepository = crawlersRepository;
//            _blocksDeduplicationRepository = blocksDeduplicationRepository;

//            Id = stopBlock.HasValue ? $"{startBlock}-{stopBlock}" : $"{startBlock}-*";
//        }

//        public async Task StartAsync()
//        {
//            // If no block was expected, then start from the scratch
//            var blockExpectation = await _crawlersRepository.GetOrDefaultAsync(_blockchainType, Id);

//            if (blockExpectation == null)
//            {
//                blockExpectation = new BlockExpectation(_startBlock);
//                await _crawlersRepository.AddIfNotExistAsync(_blockchainType, Id, blockExpectation);
//            }
            
//            await _blocksReaderApi.SendAsync(new ReadBlockCommand(blockExpectation.Number));
//        }

//        public bool CanProcess(long blockNumber)
//        {
//            return blockNumber >= _startBlock && (!_stopBlock.HasValue || blockNumber < _stopBlock.Value);
//        }

//        public async Task ChooseDirectionAsync(BlockHeader block)
//        {
//            // TODO: Have to:
//            // 1. read state, make a decision, send command
//            // 2. update state in the command handler, publish event

//            if (await _blocksDeduplicationRepository.IsProcessedAsync(block.Id))
//            {
//                return;
//            }

//            var (previousBlock, blockExpectation) = await TaskExecution.WhenAll
//            (
//                _blockHeadersRepository.GetOrDefaultAsync(block.Number - 1),
//                _crawlersRepository.GetOrDefaultAsync(TODO, Id)
//            );

//            if (blockExpectation != null && block.Number != blockExpectation.Number)
//            {
//                throw new InvalidOperationException($"Disordered block: [{block.Number}], expected block: [{blockExpectation.Number}]");
//            }

//            // --

//            var nextBlockExpectation = previousBlock == null || block.PreviousBlockId == previousBlock.Id
//                ? await MoveForwardAsync(blockExpectation, block)
//                : await MoveBackwardAsync(blockExpectation, block, previousBlock);

//            if (!_stopBlock.HasValue || nextBlockExpectation.Number < _stopBlock.Value)
//            {
//                await _blocksReaderApi.SendAsync(new ReadBlockCommand(nextBlockExpectation.Number));
//            }

//            _chaosKitty.Meow(block.Id);

//            await _blocksDeduplicationRepository.MarkAsProcessedAsync(block.Id);
//        }

//        private async Task<BlockExpectation> MoveForwardAsync(BlockExpectation blockExpectation, BlockHeader block)
//        {          
//            var skippedBlocksNumber = await SkipAlreadyReadBlocks(block);
//            var nextBlockExpectation = blockExpectation.Skip(skippedBlocksNumber);

//            await Task.WhenAll
//            (
//                _blockHeadersRepository.AddIfNotExistAsync(block),
//                _crawlersRepository.AddIfNotExistAsync(TODO, Id, nextBlockExpectation)
//            );

//            _chaosKitty.Meow(block.Id);

//            return nextBlockExpectation;
//        }

//        private async Task<long> SkipAlreadyReadBlocks(BlockHeader block)
//        {
//            var nextBlockNumber = block.Number;
//            var currentBlock = block;

//            while (true)
//            {
//                nextBlockNumber++;

//                if (_stopBlock.HasValue && nextBlockNumber >= _stopBlock)
//                {
//                    break;
//                }

//                var storedNextBlock = await _blockHeadersRepository.GetOrDefaultAsync(nextBlockNumber);

//                if (storedNextBlock == null)
//                {
//                    break;
//                }

//                // Removes already stored blocks, which belongs to another chain.
//                // For example, if chain was switched during the backward turn, thus
//                // already read on the backward turn blocks are belongs to the stale chain.

//                if (storedNextBlock.PreviousBlockId != currentBlock.Id)
//                {
//                    await Task.WhenAll
//                    (
//                        _blockHeadersRepository.RemoveAsync(storedNextBlock),
//                        _blocksDeduplicationRepository.MarkAsNotProcessedAsync(storedNextBlock.Id)
//                    );

//                    _chaosKitty.Meow(block.Id);

//                    await _contractEventsPublisher.PublishAsync(new BlockRolledBackEvent
//                    {
//                        BlockchainType = _blockchainType,
//                        BlockNumber = storedNextBlock.Number,
//                        BlockId = storedNextBlock.Id,
//                        PreviousBlockId = storedNextBlock.PreviousBlockId
//                    });

//                    break;
//                }

//                currentBlock = storedNextBlock;
//            }

//            return nextBlockNumber - block.Number;
//        }

//        private async Task<BlockExpectation> MoveBackwardAsync(BlockExpectation blockExpectation, BlockHeader block, BlockHeader previousBlock)
//        {
//            // TODO: This is not idempotent. Should be processed in independent command handler
//            var nextBlockToRead = blockExpectation.Previous();

//            var removePreviousBlockTask = _blockHeadersRepository.RemoveAsync(previousBlock);
//            var markPreviousBlockAsNotProcessedTask = _blocksDeduplicationRepository.MarkAsNotProcessedAsync(previousBlock.Id);
//            var saveBlockTask = _blockHeadersRepository.AddIfNotExistAsync(block);

//            await Task.WhenAll
//            (
//                removePreviousBlockTask,
//                markPreviousBlockAsNotProcessedTask,
//                saveBlockTask,
//                _crawlersRepository.AddIfNotExistAsync(TODO, Id, nextBlockToRead)
//            );

//            _chaosKitty.Meow(block.Id);

//            await _contractEventsPublisher.PublishAsync(new BlockRolledBackEvent
//            {
//                BlockchainType = _blockchainType,
//                BlockNumber = previousBlock.Number,
//                BlockId = previousBlock.Id,
//                PreviousBlockId = previousBlock.PreviousBlockId
//            });

//            return nextBlockToRead;
//        }
//    }
//}
