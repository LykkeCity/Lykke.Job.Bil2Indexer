using System.Threading.Tasks;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public class ChainHeadReducedEventsHandler : IMessageHandler<ChainHeadReducedEvent>
    {
        public Task HandleAsync(ChainHeadReducedEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            replyPublisher.Publish(new RollbackBlockCommand
            {
                BlockchainType = evt.BlockchainType,
                BlockNumber = evt.ToBlockNumber + 1,
                BlockId = evt.BlockIdToRollback,
                PreviousBlockId = evt.ToBlockId
            });

            return Task.CompletedTask;
        }
    }
}
