using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;
using Lykke.Job.Bil2Indexer.Infrastructure;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public class BlockAssembledEventsHandler : IMessageHandler<BlockAssembledEvent>
    {
        private readonly ICrawlersManager _crawlersManager;
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly IntegrationSettingsProvider _settingsProvider;
        private readonly IChainHeadsRepository _chainHeadsRepository;
        private readonly IntegrationSettingsProvider _integrationSettingsProvider;
        private readonly ILog _log;

        public BlockAssembledEventsHandler(
            ILogFactory logFactory,
            ICrawlersManager crawlersManager,
            IBlockHeadersRepository blockHeadersRepository,
            IntegrationSettingsProvider settingsProvider,
            IChainHeadsRepository chainHeadsRepository,
            IntegrationSettingsProvider integrationSettingsProvider)
        {
            _log = logFactory.CreateLog(this);
            _crawlersManager = crawlersManager;
            _blockHeadersRepository = blockHeadersRepository;
            _settingsProvider = settingsProvider;
            _chainHeadsRepository = chainHeadsRepository;
            _integrationSettingsProvider = integrationSettingsProvider;
        }

        public async Task<MessageHandlingResult> HandleAsync(BlockAssembledEvent evt, MessageHeaders headers,
            IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var newBlock = await _blockHeadersRepository.GetAsync(evt.BlockchainType, evt.BlockId);

            var (previousBlock, crawler) = await TaskExecution.WhenAll
            (
                _blockHeadersRepository.GetOrDefaultAsync(evt.BlockchainType, newBlock.Number - 1),
                _crawlersManager.GetCrawlerAsync(evt.BlockchainType, newBlock.Number)
            );

            var crawlerCorrelationId = crawler.GetCorrelationId();

            if (messageCorrelationId.IsLegacyRelativeTo(crawlerCorrelationId))
            {
                // The message is legacy, it already was processed for sure, we can ignore it.
                _log.LogLegacyMessage(evt, headers);

                return MessageHandlingResult.Success();
            }

            if (messageCorrelationId.IsPrematureRelativeTo(crawlerCorrelationId))
            {
                // The message is premature, it can't be processed yet, we should retry it later.
                return MessageHandlingResult.TransientFailure();
            }

            if (newBlock.IsAssembled)
            {
                var crawlingDirection = crawler.EvaluateDirection(previousBlock, newBlock);
                var chainHead = await _chainHeadsRepository.GetAsync(evt.BlockchainType);

                switch (crawlingDirection)
                {
                    case CrawlingDirection.Forward:
                        MoveForward(evt, replyPublisher, crawler, chainHead, crawlerCorrelationId, newBlock);
                        break;

                    case CrawlingDirection.Backward:
                        MoveBackward(evt, replyPublisher, crawler, chainHead, crawlerCorrelationId, newBlock,
                            previousBlock);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(crawlingDirection), crawlingDirection, "Unknown value");
                }
            }

            return MessageHandlingResult.Success();
        }

        private void MoveForward(
            BlockAssembledEvent evt, 
            IMessagePublisher replyPublisher, 
            Crawler crawler,
            ChainHead chainHead,
            CrawlerCorrelationId crawlerCorrelationId,
            BlockHeader newBlock)
        {
            var settings = _settingsProvider.Get(evt.BlockchainType);
            var nextBlockNumber = crawler.EvaluateNextBlockToMoveForward(newBlock);
            
            if (settings.Capabilities.TransferModel == BlockchainTransferModel.Amount)
            {
                if (chainHead.IsFollowCrawler || newBlock.Number == _integrationSettingsProvider.Get(evt.BlockchainType).Capabilities.FirstBlockNumber)
                {
                    replyPublisher.Publish
                    (
                        new ExtendChainHeadCommand
                        {
                            BlockchainType = evt.BlockchainType,
                            ToBlockNumber = newBlock.Number,
                            ToBlockId = newBlock.Id,
                        },
                        chainHead.GetCorrelationId(crawlerCorrelationId).ToString()
                    );
                }
            }
            else if (settings.Capabilities.TransferModel == BlockchainTransferModel.Coins)
            {
                replyPublisher.Publish
                (
                    new ExecuteTransferCoinsBlockCommand
                    {
                        BlockchainType = evt.BlockchainType,
                        BlockId = newBlock.Id,
                        HaveToExecuteEntireBlock = chainHead.IsFollowCrawler || chainHead.BlockNumber + 1 == newBlock.Number,
                        TriggeredBy = BlockExecutionTrigger.Crawler
                    },
                    chainHead.GetCorrelationId(crawlerCorrelationId).ToString()
                );
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(settings.Capabilities.TransferModel), settings.Capabilities.TransferModel, "Unknown transfer model");
            }

            if (crawler.HaveToWaitFor(chainHead, CrawlingDirection.Forward))
            {
                replyPublisher.Publish(new WaitForChainHeadCommand
                {
                    BlockchainType = evt.BlockchainType,
                    Direction = CrawlingDirection.Forward,
                    TargetBlockNumber = nextBlockNumber,
                    OutdatedBlockNumber = 0,
                    OutdatedBlockId = null,
                    TriggeredByBlockId = newBlock.Id
                });
            }
            else
            {
                replyPublisher.Publish(new MoveCrawlerCommand
                {
                    BlockchainType = evt.BlockchainType, 
                    NextBlockNumber = nextBlockNumber
                });
            }
        }

        private void MoveBackward(BlockAssembledEvent evt,
            IMessagePublisher replyPublisher,
            Crawler crawler,
            ChainHead chainHead,
            CrawlerCorrelationId crawlerCorrelationId,
            BlockHeader newBlock,
            BlockHeader previousBlock)
        {
            var nextChainHeadBlockNumber = previousBlock.Number - 1;
            
            if (crawler.HaveToWaitFor(chainHead, CrawlingDirection.Backward))
            {
                replyPublisher.Publish(new WaitForChainHeadCommand
                {
                    BlockchainType = evt.BlockchainType,
                    Direction = CrawlingDirection.Backward,
                    TargetBlockNumber = nextChainHeadBlockNumber,
                    OutdatedBlockId = previousBlock.Id,
                    // it's possible that previous block can't be processed by the crawler (because of crawler range bounds),
                    // but we ignores this case since this is very unlikely that chain fork can intersect ranges of different crawlers.
                    OutdatedBlockNumber = previousBlock.Number,
                    TriggeredByBlockId = newBlock.Id
                });
            }
            else
            {
                replyPublisher.Publish
                (
                    new ReduceChainHeadCommand
                    {
                        BlockchainType = evt.BlockchainType,
                        ToBlockNumber = nextChainHeadBlockNumber,
                        OutdatedBlockId = previousBlock.Id,
                        // it's possible that previous block can't be processed by the crawler (because of crawler range bounds),
                        // but we ignores this case since this is very unlikely that chain fork can intersect ranges of different crawlers.
                        OutdatedBlockNumber = previousBlock.Number,
                        TriggeredByBlockId = newBlock.Id
                    },
                    chainHead.GetCorrelationId(crawlerCorrelationId).ToString()
                );
            }
        }
    }
}
