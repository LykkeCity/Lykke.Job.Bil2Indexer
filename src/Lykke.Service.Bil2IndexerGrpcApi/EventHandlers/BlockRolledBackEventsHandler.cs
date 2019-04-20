using System;
using System.Threading.Tasks;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Contract.Events;

namespace Lykke.Service.Bil2IndexerGrpcApi.EventHandlers
{
    public class BlockRolledBackEventsHandler : IMessageHandler<BlockRolledBackEvent>
    {
        public Task<MessageHandlingResult> HandleAsync(BlockRolledBackEvent evt, MessageHeaders headers, IMessagePublisher _)
        {
            Console.WriteLine($"BlockRolledBackEvent :{Newtonsoft.Json.JsonConvert.SerializeObject(evt)}");

            return Task.FromResult(MessageHandlingResult.Success());
        }
    }
}
