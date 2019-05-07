using System;
using System.Threading.Tasks;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Contract.Events;
using Lykke.Service.Bil2IndexerGrpcApi.Services;

namespace Lykke.Service.Bil2IndexerGrpcApi.EventHandlers
{
    public class BlockRolledBackEventsHandler : IMessageHandler<BlockRolledBackEvent>
    {
        private readonly IndexerApiImpl _indexerApi;

        public BlockRolledBackEventsHandler(IndexerApiImpl indexerApi)
        {
            _indexerApi = indexerApi;
        }

        public Task<MessageHandlingResult> HandleAsync(BlockRolledBackEvent evt, MessageHeaders headers, IMessagePublisher _)
        {
            Console.WriteLine($"BlockRolledBackEvent :{Newtonsoft.Json.JsonConvert.SerializeObject(evt)}");

            _indexerApi.Publish(evt);

            return Task.FromResult(MessageHandlingResult.Success());
        }
    }
}
