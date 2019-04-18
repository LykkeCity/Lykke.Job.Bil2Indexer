using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Contract.Events;

namespace Lykke.Service.Bil2IndexerGrpcApi.EventHandlers
{
    [UsedImplicitly]
    public class LastIrreversibleBlockUpdatedEventsHandler : IMessageHandler<LastIrreversibleBlockUpdatedEvent>
    {
        public Task HandleAsync(LastIrreversibleBlockUpdatedEvent evt, MessageHeaders headers, IMessagePublisher _)
        {
            Console.WriteLine($"LastIrreversibleBlockUpdatedEvent :{Newtonsoft.Json.JsonConvert.SerializeObject(evt)}");

            return Task.CompletedTask;
        }
    }
}
