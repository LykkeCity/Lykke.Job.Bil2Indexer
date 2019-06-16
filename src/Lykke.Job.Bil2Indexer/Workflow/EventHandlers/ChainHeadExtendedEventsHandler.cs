using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Contract.Events;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;
using Lykke.Job.Bil2Indexer.Infrastructure;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Workflow.Commands;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public class ChainHeadExtendedEventsHandler : IMessageHandler<ChainHeadExtendedEvent>
    {
        private readonly IChainHeadsRepository _chainHeadsRepository;
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly IntegrationSettingsProvider _settingsProvider;
        private readonly ICrawlersManager _crawlersManager;
        private readonly ILog _log;

        public ChainHeadExtendedEventsHandler(
            ILogFactory logFactory,
            IChainHeadsRepository chainHeadsRepository,
            IBlockHeadersRepository blockHeadersRepository,
            IntegrationSettingsProvider settingsProvider,
            ICrawlersManager crawlersManager)
        {
            _log = logFactory.CreateLog(this);
            _chainHeadsRepository = chainHeadsRepository;
            _blockHeadersRepository = blockHeadersRepository;
            _settingsProvider = settingsProvider;
            _crawlersManager = crawlersManager;
        }

        public async Task<MessageHandlingResult> HandleAsync(ChainHeadExtendedEvent evt, MessageHeaders headers,
            IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = ChainHeadCorrelationId.Parse(headers.CorrelationId);
            var (chainHead, infiniteCrawler) = await TaskExecution.WhenAll
            (
                _chainHeadsRepository.GetAsync(evt.BlockchainType),
                _crawlersManager.GetInfiniteCrawlerAsync(evt.BlockchainType)
            );
            var chainHeadCorrelationId = chainHead.GetCorrelationId();

            if (messageCorrelationId.IsLegacyRelativeTo(chainHeadCorrelationId, chainHead.Mode))
            {
                // The message is legacy, it already was processed for sure, we can ignore it.
                _log.LogLegacyMessage(evt, headers);

                return MessageHandlingResult.Success();
            }

            if (messageCorrelationId.IsPrematureRelativeTo(chainHeadCorrelationId, chainHead.Mode))
            {
                // The message is premature, it can't be processed yet, we should retry it later.
                return MessageHandlingResult.TransientFailure();
            }

            if (chainHead.IsFollowCrawler)
            {
                if (chainHead.HaveToDetachFrom(infiniteCrawler))
                {
                    replyPublisher.Publish(new DetachChainHeadFromCrawlerCommand
                    {
                        BlockchainType = evt.BlockchainType,
                        BlockNumber = evt.BlockNumber
                    });
                }
            }

            if (chainHead.IsCatchCrawlerUp)
            {
                var settings = _settingsProvider.Get(evt.BlockchainType);
                var nextBlockNumber = evt.BlockNumber + 1;
                var nextBlock = await _blockHeadersRepository.GetOrDefaultAsync(evt.BlockchainType, nextBlockNumber);

                if (nextBlock == null)
                {
                    if (chainHead.HaveToAttachTo(infiniteCrawler))
                    {
                        replyPublisher.Publish(new AttachChainHeadToCrawlerCommand
                        {
                            BlockchainType = evt.BlockchainType, 
                            CrawlerSequence = infiniteCrawler.Sequence
                        });
                    }
                    else
                    {
                        // Chain head need to wait for the next block to be received.
                        return MessageHandlingResult.TransientFailure(TimeSpan.FromSeconds(5));
                    }
                }
                else
                {
                    return ChainHeadExtendingHelper.PerformExtendingFlow(replyPublisher, settings, nextBlock);
                }
            }

            return MessageHandlingResult.Success();
        }
    }
}
