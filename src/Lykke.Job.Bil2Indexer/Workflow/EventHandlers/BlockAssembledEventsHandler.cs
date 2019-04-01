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
    public class BlockAssembledEventsHandler : IMessageHandler<BlockAssembledEvent>
    {
        private readonly ICrawlersManager _crawlersManager;
        private readonly IBlockHeadersRepository _blockHeadersRepository;

        public BlockAssembledEventsHandler(
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
            
            replyPublisher.Publish(new ExecuteTransferCoinsBlockCommand
            {
                BlockchainType = evt.BlockchainType,
                BlockId = newBlock.Id,
                BlockVersion = newBlock.Version
            });
        }
    }
}
