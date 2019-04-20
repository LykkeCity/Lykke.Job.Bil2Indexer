using System;
using System.Threading.Tasks;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Service.Bil2IndexerGrpcApi.Services;

namespace Lykke.Service.Bil2IndexerGrpcApi.EventHandlers
{
    public class TransactionExecutedEventsHandler : IMessageHandler<Job.Bil2Indexer.Contract.Events.TransactionExecutedEvent>
    {
        private readonly IndexerApiImpl _indexerApi;

        public TransactionExecutedEventsHandler(IndexerApiImpl indexerApi)
        {
            _indexerApi = indexerApi;
        }
        
        public Task<MessageHandlingResult> HandleAsync(Lykke.Job.Bil2Indexer.Contract.Events.TransactionExecutedEvent evt, MessageHeaders headers, IMessagePublisher _)
        {
            Console.WriteLine($"TransactionExecutedEvent :{Newtonsoft.Json.JsonConvert.SerializeObject(evt)}");

            _indexerApi.Publish(evt);

            return Task.FromResult(MessageHandlingResult.Success());
        }
    }
}
