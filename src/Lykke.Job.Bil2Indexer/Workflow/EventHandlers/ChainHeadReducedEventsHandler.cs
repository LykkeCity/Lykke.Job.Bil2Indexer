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
using Lykke.Job.Bil2Indexer.Workflow.Commands;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public class ChainHeadReducedEventsHandler : IMessageHandler<ChainHeadReducedEvent>
    {
        private readonly IChainHeadsRepository _chainHeadsRepository;
        private readonly ICrawlersManager _crawlersManager;
        private readonly ILog _log;

        public ChainHeadReducedEventsHandler(
            ILogFactory logFactory,
            IChainHeadsRepository chainHeadsRepository,
            ICrawlersManager crawlersManager)
        {
            _chainHeadsRepository = chainHeadsRepository;
            _crawlersManager = crawlersManager;
            _log = logFactory.CreateLog(this);
        }

        public async Task<MessageHandlingResult> HandleAsync(ChainHeadReducedEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = ChainHeadCorrelationId.Parse(headers.CorrelationId);
            var chainHead = await _chainHeadsRepository.GetAsync(evt.BlockchainType);
            var chainHeadCorrelationId = chainHead.GetCorrelationId();

            if (messageCorrelationId.IsLegacyRelativeTo(chainHeadCorrelationId))
            {
                // The message is legacy, it already was processed for sure, we can ignore it.
                _log.LogLegacyMessage(evt, headers);

                return MessageHandlingResult.Success();
            }

            if (messageCorrelationId.IsPrematureRelativeTo(chainHeadCorrelationId))
            {
                // The message is premature, it can't be processed yet, we should retry it later.
                return MessageHandlingResult.TransientFailure();
            }

            var crawlerConfiguration = _crawlersManager.GetInfiniteCrawlerConfiguration(evt.BlockchainType);
            var crawlerCorrelationId = new CrawlerCorrelationId
            (
                evt.BlockchainType,
                crawlerConfiguration,
                chainHead.CrawlerSequence
            );

            replyPublisher.Publish
            (
                new MoveCrawlerCommand
                {
                    BlockchainType = evt.BlockchainType,
                    NextBlockNumber = evt.OutdatedBlockNumber

                },
                crawlerCorrelationId.ToString()
            );

            return MessageHandlingResult.Success();
        }
    }
}
