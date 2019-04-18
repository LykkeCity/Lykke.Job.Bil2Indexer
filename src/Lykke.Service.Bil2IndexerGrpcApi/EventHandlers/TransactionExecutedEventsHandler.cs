using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Contract.Events;

namespace Lykke.Service.Bil2IndexerGrpcApi.EventHandlers
{
    [UsedImplicitly]
    public class TransactionExecutedEventsHandler : IMessageHandler<TransactionExecutedEvent>
    {
        public Task HandleAsync(TransactionExecutedEvent evt, MessageHeaders headers, IMessagePublisher _)
        {
            Console.WriteLine($"TransactionExecutedEvent :{Newtonsoft.Json.JsonConvert.SerializeObject(evt)}");

            return Task.CompletedTask;
        }
    }
}
