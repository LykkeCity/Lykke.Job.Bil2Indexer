using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Contract.Events;
using Lykke.Service.Bil2IndexerGrpcApi.Services;
using System.Threading.Tasks;

namespace Lykke.Service.Bil2IndexerGrpcApi.EventHandlers
{
    public class TransactionFailedEventsHandler : IMessageHandler<TransactionFailedEvent>
    {
        private readonly IndexerApiImpl _indexerApi;

        public TransactionFailedEventsHandler(IndexerApiImpl indexerApi)
        {
            _indexerApi = indexerApi;
        }

        public Task<MessageHandlingResult> HandleAsync(TransactionFailedEvent evt, MessageHeaders headers, IMessagePublisher _)
        {
            _indexerApi.Publish(evt);

            return Task.FromResult(MessageHandlingResult.Success());
        }
    }
}
