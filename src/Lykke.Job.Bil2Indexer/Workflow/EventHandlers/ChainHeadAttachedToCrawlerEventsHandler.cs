using System.Threading.Tasks;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public class ChainHeadAttachedToCrawlerEventsHandler : IMessageHandler<ChainHeadAttachedToCrawlerEvent>
    {
        public Task<MessageHandlingResult> HandleAsync(ChainHeadAttachedToCrawlerEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            return Task.FromResult(MessageHandlingResult.Success());
        }
    }
}
