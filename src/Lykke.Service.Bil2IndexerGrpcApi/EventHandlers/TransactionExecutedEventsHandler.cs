using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Contract.Events;
using Lykke.Service.Bil2IndexerGrpcApi.Services;

namespace Lykke.Service.Bil2IndexerGrpcApi.EventHandlers
{
    [UsedImplicitly]
    public class TransactionExecutedEventsHandler : IMessageHandler<Lykke.Job.Bil2Indexer.Contract.Events.TransactionExecutedEvent>
    {
        private readonly IndexerApiImpl _indexerApi;
        public TransactionExecutedEventsHandler(IndexerApiImpl indexerApi)
        {
            _indexerApi = indexerApi;
        }


        public Task HandleAsync(Lykke.Job.Bil2Indexer.Contract.Events.TransactionExecutedEvent evt, MessageHeaders headers, IMessagePublisher _)
        {
            Console.WriteLine($"TransactionExecutedEvent :{Newtonsoft.Json.JsonConvert.SerializeObject(evt)}");

            _indexerApi.Publish(evt);

            return Task.CompletedTask;
        }
    }
}
