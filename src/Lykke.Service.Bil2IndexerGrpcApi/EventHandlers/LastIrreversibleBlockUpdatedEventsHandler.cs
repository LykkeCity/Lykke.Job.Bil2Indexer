using System;
using System.Threading.Tasks;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Contract.Events;

namespace Lykke.Service.Bil2IndexerGrpcApi.EventHandlers
{
    public class LastIrreversibleBlockUpdatedEventsHandler : IMessageHandler<LastIrreversibleBlockUpdatedEvent>
    {
        public Task<MessageHandlingResult> HandleAsync(LastIrreversibleBlockUpdatedEvent evt, MessageHeaders headers, IMessagePublisher _)
        {
            Console.WriteLine($"LastIrreversibleBlockUpdatedEvent :{Newtonsoft.Json.JsonConvert.SerializeObject(evt)}");

            return Task.FromResult(MessageHandlingResult.Success());
        }
    }
}
