using System;
using System.Threading.Tasks;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;
using Lykke.Job.Bil2Indexer.Infrastructure;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public class BlockBuildingEventsHandler :
        IMessageHandler<BlockAssembledEvent>
    {
        private readonly ICrawlersManager _crawlersManager;
        private readonly IBlockHeadersRepository _blockHeadersRepository;

        public BlockBuildingEventsHandler(
            ICrawlersManager crawlersManager,
            IBlockHeadersRepository blockHeadersRepository)
        {
            _crawlersManager = crawlersManager;
            _blockHeadersRepository = blockHeadersRepository;
        }

        public async Task HandleAsync(BlockAssembledEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);

            var newBlock = await _blockHeadersRepository.GetAsync(evt.BlockchainType, evt.BlockId);
            var (previousBlock, crawler) = await TaskExecution.WhenAll
            (
                _blockHeadersRepository.GetOrDefaultAsync(evt.BlockchainType, newBlock.Number - 1),
                _crawlersManager.GetCrawlerAsync(evt.BlockchainType, newBlock.Number)
            );

            if (!crawler.GetCorrelationId().Equals(messageCorrelationId))
            {
                // Disordered message, we should ignore it.
                return;
            }

            long nextBlockNumber;
            var crawlingDirection = crawler.EvaluateDirection(previousBlock, newBlock);

            switch (crawlingDirection)
            {
                case CrawlingDirection.Forward:
                    nextBlockNumber = await crawler.EvaluateNextBlockToMoveForwardAsync
                    (
                        newBlock,
                        blockNumber => _blockHeadersRepository.GetOrDefaultAsync(evt.BlockchainType, blockNumber),
                        blockToRollback => replyPublisher.Publish
                        (
                            new RollbackBlockCommand
                            {
                                BlockchainType = evt.BlockchainType,
                                BlockId = blockToRollback.Id,
                                BlockNumber = blockToRollback.Number,
                                PreviousBlockId = blockToRollback.PreviousBlockId
                            }
                        )
                    );
                    break;

                case CrawlingDirection.Backward:
                    nextBlockNumber = crawler.EvaluateNextBlockToMoveBackward
                    (
                        newBlock,
                        previousBlock,
                        blockToRollback => replyPublisher.Publish
                        (
                            new RollbackBlockCommand
                            {
                                BlockchainType = evt.BlockchainType,
                                BlockId = blockToRollback.Id,
                                BlockNumber = blockToRollback.Number,
                                PreviousBlockId = blockToRollback.PreviousBlockId
                            }
                        )
                    );
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(crawlingDirection), crawlingDirection, "Unknown value");
            }
            
            replyPublisher.Publish(new MoveCrawlerCommand
            {
                BlockchainType = evt.BlockchainType,
                NextBlockNumber = nextBlockNumber
            });
            
            //

            //if (await _blockFlagsRepository.IsSetAsync(evt.BlockchainType, evt.BlockId, BlockFlags.RolledBack))
            //{
            //    return;
            //}

            //var oldCompletedBlock = await _latestCompletedBlockRepository.GetOrDefaultAsync(evt.BlockchainType);
            //var oldCompletedBlockNumber = oldCompletedBlock?.Number;
            //var assembledBlockHeader = await _blockHeadersRepository.GetAsync(evt.BlockchainType, evt.BlockId);
            //var assembledBlockNumber = assembledBlockHeader.Number;
            //BlockHeader newCompletedBlockHeader = null;

            //if (oldCompletedBlockNumber != null && oldCompletedBlockNumber < assembledBlockNumber - 1)
            //{
            //    for (var blockNumber = oldCompletedBlockNumber.Value + 1; blockNumber < assembledBlockNumber; ++blockNumber)
            //    {
            //        var blockHeader = await _blockHeadersRepository.GetOrDefaultAsync(evt.BlockchainType, blockNumber);

            //        if (blockHeader == null)
            //        {
            //            break;
            //        }

            //        if (!await _blockFlagsRepository.IsSetAsync(evt.BlockchainType, blockHeader.Id, BlockFlags.Assembled) ||
            //            await _blockFlagsRepository.IsSetAsync(evt.BlockchainType, blockHeader.Id, BlockFlags.RolledBack))
            //        {
            //            break;
            //        }

            //        newCompletedBlockHeader = blockHeader;

            //        replyPublisher.Publish(new UpdateCompletedBlockCommand
            //        {
            //            BlockchainType = evt.BlockchainType,
            //            NewCompletedBlockNumber = blockHeader.Number,
            //            NewCompletedBlockId = blockHeader.Id,
            //            OldCompletedBlockNumber = oldCompletedBlockNumber,
            //            OldCompletedBlockId = oldCompletedBlock?.Id
            //        });
            //    }
            //}
            //else if(oldCompletedBlockNumber != null)
            //{
            //    newCompletedBlockHeader = assembledBlockHeader;
            //}
            //else
            //{
            //    var firstBlockNumber = _integrationSettingsProvider.Get(evt.BlockchainType).Capabilities.FirstBlockNumber;

            //    if (assembledBlockHeader.Number == firstBlockNumber)
            //    {
            //        newCompletedBlockHeader = assembledBlockHeader;
            //    }
            //}

            //if (newCompletedBlockHeader != null)
            //{
            //    replyPublisher.Publish(new UpdateCompletedBlockCommand
            //    {
            //        BlockchainType = evt.BlockchainType,
            //        NewCompletedBlockNumber = newCompletedBlockHeader.Number,
            //        NewCompletedBlockId = newCompletedBlockHeader.Id,
            //        OldCompletedBlockNumber = oldCompletedBlockNumber,
            //        OldCompletedBlockId = oldCompletedBlock?.Id
            //    });
            //}
        }
    }
}
