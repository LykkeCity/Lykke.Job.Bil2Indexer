﻿using System;
using System.Threading.Tasks;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Contract.Events;
using Lykke.Service.Bil2IndexerGrpcApi.Services;

namespace Lykke.Service.Bil2IndexerGrpcApi.EventHandlers
{
    public class LastIrreversibleBlockUpdatedEventsHandler : IMessageHandler<LastIrreversibleBlockUpdatedEvent>
    {
        private readonly IndexerApiImpl _indexerApi;

        public LastIrreversibleBlockUpdatedEventsHandler(IndexerApiImpl indexerApi)
        {
            _indexerApi = indexerApi;
        }

        public Task<MessageHandlingResult> HandleAsync(LastIrreversibleBlockUpdatedEvent evt, MessageHeaders headers, IMessagePublisher _)
        {
            Console.WriteLine($"LastIrreversibleBlockUpdatedEvent :{Newtonsoft.Json.JsonConvert.SerializeObject(evt)}");

            _indexerApi.Publish(evt);

            return Task.FromResult(MessageHandlingResult.Success());
        }
    }
}
