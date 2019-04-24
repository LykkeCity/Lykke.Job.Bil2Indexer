using System.Threading.Tasks;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public class BlockExecutedEventsHandler : IMessageHandler<BlockExecutedEvent>
    {
        private readonly IChainHeadsRepository _chainHeadsRepository;

        public BlockExecutedEventsHandler(IChainHeadsRepository chainHeadsRepository)
        {
            _chainHeadsRepository = chainHeadsRepository;
        }

        public async Task<MessageHandlingResult> HandleAsync(BlockExecutedEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = ChainHeadCorrelationId.Parse(headers.CorrelationId);
            var chainHead = await _chainHeadsRepository.GetAsync(evt.BlockchainType);
            var chainHeadCorrelationId = chainHead.GetCorrelationId();

            if (messageCorrelationId.IsLegacyRelativeTo(chainHeadCorrelationId))
            {
                // The message is legacy, it already was processed for sure, we can ignore it.
                return MessageHandlingResult.Success();
            }

            if (messageCorrelationId.IsPrematureRelativeTo(chainHeadCorrelationId))
            {
                // The message is premature, it can't be processed yet, we should retry it later.
                return MessageHandlingResult.TransientFailure();
            }

            if (chainHead.CanExtendTo(evt.BlockNumber))
            {
                replyPublisher.Publish(new ExtendChainHeadCommand
                {
                    BlockchainType = evt.BlockchainType,
                    ToBlockNumber = evt.BlockNumber,
                    ToBlockId = evt.BlockId
                });
            }

            return MessageHandlingResult.Success();
        }
    }
}
