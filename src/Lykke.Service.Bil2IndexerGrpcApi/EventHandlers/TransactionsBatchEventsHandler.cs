using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Service.Bil2IndexerGrpcApi.Services;
using System.Threading.Tasks;

namespace Lykke.Service.Bil2IndexerGrpcApi.EventHandlers
{
    public class TransactionsBatchEventsHandler : IMessageHandler<Job.Bil2Indexer.Contract.Events.TransactionsBatchEvent>
    {
        private readonly IndexerApiImpl _indexerApi;

        public TransactionsBatchEventsHandler(IndexerApiImpl indexerApi)
        {
            _indexerApi = indexerApi;
        }

        public Task<MessageHandlingResult> HandleAsync(Job.Bil2Indexer.Contract.Events.TransactionsBatchEvent evt, MessageHeaders headers, IMessagePublisher _)
        {
            _indexerApi.Publish(evt);

            return Task.FromResult(MessageHandlingResult.Success());
        }
    }
}
