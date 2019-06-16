using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Infrastructure;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public class ChainHeadDetachedFromCrawlerEventsHandler : IMessageHandler<ChainHeadDetachedFromCrawlerEvent>
    {
        private readonly IChainHeadsRepository _chainHeadsRepository;
        private readonly IntegrationSettingsProvider _settingsProvider;
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly ILog _log;

        public ChainHeadDetachedFromCrawlerEventsHandler(
            ILogFactory logFactory,
            IChainHeadsRepository chainHeadsRepository,
            IntegrationSettingsProvider settingsProvider,
            IBlockHeadersRepository blockHeadersRepository)
        {
            _chainHeadsRepository = chainHeadsRepository;
            _settingsProvider = settingsProvider;
            _blockHeadersRepository = blockHeadersRepository;
            _log = logFactory.CreateLog(this);
        }

        public async Task<MessageHandlingResult> HandleAsync(ChainHeadDetachedFromCrawlerEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = ChainHeadCorrelationId.Parse(headers.CorrelationId);
            var chainHead = await _chainHeadsRepository.GetAsync(evt.BlockchainType);
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

            var settings = _settingsProvider.Get(evt.BlockchainType);
            var nextBlockNumber = evt.BlockNumber + 1;
            var nextBlock = await _blockHeadersRepository.GetAsync(evt.BlockchainType, nextBlockNumber);

            return ChainHeadExtendingHelper.PerformExtendingFlow(replyPublisher, settings, nextBlock);
        }
    }
}
