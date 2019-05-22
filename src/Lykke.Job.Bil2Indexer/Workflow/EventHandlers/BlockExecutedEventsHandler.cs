using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Infrastructure;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public class BlockExecutedEventsHandler : IMessageHandler<BlockExecutedEvent>
    {
        private readonly IChainHeadsRepository _chainHeadsRepository;
        private readonly IntegrationSettingsProvider _integrationSettingsProvider;
        private readonly ILog _log;

        public BlockExecutedEventsHandler(
            ILogFactory logFactory,
            IChainHeadsRepository chainHeadsRepository,
            IntegrationSettingsProvider integrationSettingsProvider)
        {
            _log = logFactory.CreateLog(this);
            _chainHeadsRepository = chainHeadsRepository;
            _integrationSettingsProvider = integrationSettingsProvider;
        }

        public async Task<MessageHandlingResult> HandleAsync(BlockExecutedEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = ChainHeadCorrelationId.Parse(headers.CorrelationId);
            
            if (evt.TriggeredBy == BlockExecutionTrigger.Crawler && messageCorrelationId.Mode == ChainHeadMode.FollowsCrawler ||
                evt.TriggeredBy == BlockExecutionTrigger.ChainHead && messageCorrelationId.Mode == ChainHeadMode.CatchesCrawlerUp ||
                evt.BlockNumber == _integrationSettingsProvider.Get(evt.BlockchainType).Capabilities.FirstBlockNumber)
            {
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

                replyPublisher.Publish
                (
                    new ExtendChainHeadCommand
                    {
                        BlockchainType = evt.BlockchainType,
                        ToBlockNumber = evt.BlockNumber,
                        ToBlockId = evt.BlockId
                    }
                );
            }

            return MessageHandlingResult.Success();
        }
    }
}
