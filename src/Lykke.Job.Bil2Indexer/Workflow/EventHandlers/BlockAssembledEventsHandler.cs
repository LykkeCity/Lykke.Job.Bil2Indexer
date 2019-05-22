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
        private readonly ILog _log;

        public BlockAssembledEventsHandler(
            ILogFactory logFactory,
            ICrawlersManager crawlersManager,
            IBlockHeadersRepository blockHeadersRepository,
            IntegrationSettingsProvider settingsProvider,
            IChainHeadsRepository chainHeadsRepository)
        {
            _log = logFactory.CreateLog(this);
            _crawlersManager = crawlersManager;
            _blockHeadersRepository = blockHeadersRepository;
            _settingsProvider = settingsProvider;
            _chainHeadsRepository = chainHeadsRepository;
        }

        public async Task<MessageHandlingResult> HandleAsync(BlockAssembledEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
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

            var crawlingDirection = crawler.EvaluateDirection(previousBlock, newBlock);
            
            switch (crawlingDirection)
            {
                case CrawlingDirection.Forward:
                    await MoveForwardAsync(evt, replyPublisher, crawler, crawlerCorrelationId, newBlock);
                    break;

                case CrawlingDirection.Backward:
                    await MoveBackwardAsync(evt, replyPublisher, crawlerCorrelationId, newBlock, previousBlock, headers);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(crawlingDirection), crawlingDirection, "Unknown value");
            }

            return MessageHandlingResult.Success();
        }

        private async Task<MessageHandlingResult> MoveForwardAsync(
            BlockAssembledEvent evt, 
            IMessagePublisher replyPublisher, 
            Crawler crawler,
            CrawlerCorrelationId crawlerCorrelationId,
            BlockHeader newBlock)
        {
            var settings = _settingsProvider.Get(evt.BlockchainType);
            var nextBlockNumber = crawler.EvaluateNextBlockToMoveForward(newBlock);
            var chainHead = await _chainHeadsRepository.GetAsync(evt.BlockchainType);

            if (settings.Capabilities.TransferModel == BlockchainTransferModel.Amount)
            {
                if (chainHead.IsFollowCrawler)
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
                        HaveToExecuteEntireBlock = chainHead.IsFollowCrawler,
                        TriggeredBy = BlockExecutionTrigger.Crawler
                    },
                    chainHead.GetCorrelationId(crawlerCorrelationId).ToString()
                );
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(settings.Capabilities.TransferModel), settings.Capabilities.TransferModel, "Unknown transfer model");
            }

            replyPublisher.Publish(new MoveCrawlerCommand
            {
                BlockchainType = evt.BlockchainType,
                NextBlockNumber = nextBlockNumber
            });

            return MessageHandlingResult.Success();
        }

        private async Task<MessageHandlingResult> MoveBackwardAsync(BlockAssembledEvent evt,
            IMessagePublisher replyPublisher,
            CrawlerCorrelationId crawlerCorrelationId,
            BlockHeader newBlock,
            BlockHeader previousBlock, 
            MessageHeaders headers)
        {
            var chainHead = await _chainHeadsRepository.GetAsync(evt.BlockchainType);
            var nextChainHeadBlockNumber = previousBlock.Number - 1;

            if (chainHead.IsCatchCrawlerUp)
            {
                // Chain head reduction is only possible if chain head follows the crawler
                _log.Info("Backward movement is only possible if chain head follows the crawler", new
                {
                    Headers = headers,
                    Message = evt,
                    ChainHead = chainHead
                });

                return MessageHandlingResult.TransientFailure();
            }

            if (!chainHead.IsFollowCrawler)
            {
                throw new InvalidOperationException("Chain head reduction requires that chain head follows the crawler");
            }

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

            return MessageHandlingResult.Success();
        }
    }
}
