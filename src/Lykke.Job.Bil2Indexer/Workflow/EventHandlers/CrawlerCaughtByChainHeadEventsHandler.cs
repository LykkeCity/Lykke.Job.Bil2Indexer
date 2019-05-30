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
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public class CrawlerCaughtByChainHeadEventsHandler : IMessageHandler<CrawlerCaughtByChainHeadEvent>
    {
        private readonly ILog _log;
        private readonly ICrawlersManager _crawlersManager;
        private readonly IChainHeadsRepository _chainHeadsRepository;

        public CrawlerCaughtByChainHeadEventsHandler(
            ILogFactory logFactory,
            ICrawlersManager crawlersManager,
            IChainHeadsRepository chainHeadsRepository)
        {
            _log = logFactory.CreateLog(this);
            _crawlersManager = crawlersManager;
            _chainHeadsRepository = chainHeadsRepository;
        }

        public async Task<MessageHandlingResult> HandleAsync(CrawlerCaughtByChainHeadEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var crawler = await _crawlersManager.GetCrawlerAsync(evt.BlockchainType, messageCorrelationId.Configuration);
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

            if (crawler.IsIndexing)
            {
                switch (evt.Direction)
                {
                    case CrawlingDirection.Forward:
                        replyPublisher.Publish(new MoveCrawlerCommand
                        {
                            BlockchainType = evt.BlockchainType,
                            NextBlockNumber = evt.TargetBlockNumber
                        });
                        break;

                    case CrawlingDirection.Backward:
                    {
                        var chainHead = await _chainHeadsRepository.GetAsync(evt.BlockchainType);

                        replyPublisher.Publish
                        (
                            new ReduceChainHeadCommand
                            {
                                BlockchainType = evt.BlockchainType,
                                OutdatedBlockId = evt.OutdatedBlockId,
                                OutdatedBlockNumber = evt.OutdatedBlockNumber,
                                ToBlockNumber = evt.TargetBlockNumber,
                                TriggeredByBlockId = evt.TriggeredByBlockId
                            },
                            chainHead.GetCorrelationId(crawlerCorrelationId).ToString()
                        );
                        break;
                    }

                    default:
                        throw new ArgumentOutOfRangeException(nameof(evt.Direction), evt.Direction, string.Empty);
                }
            }

            return MessageHandlingResult.Success();
        }
    }
}
