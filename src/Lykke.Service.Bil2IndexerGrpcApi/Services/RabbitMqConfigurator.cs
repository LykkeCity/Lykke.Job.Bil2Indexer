using JetBrains.Annotations;
using Lykke.Bil2.RabbitMq;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Contract;
using Lykke.Job.Bil2Indexer.Contract.Events;
using Lykke.Service.Bil2IndexerGrpcApi.EventHandlers;

namespace Lykke.Service.Bil2IndexerGrpcApi.Services
{
    [UsedImplicitly]
    public class RabbitMqConfigurator
    {
        private readonly IRabbitMqEndpoint _endpoint;

        public RabbitMqConfigurator(IRabbitMqEndpoint endpoint)
        {
            _endpoint = endpoint;
        }

        public void Configure()
        {
            _endpoint.DeclareExchange(Bil2IndexerContractExchanges.Events);

            var eventsSubscriptions = new MessageSubscriptionsRegistry()
                .Handle<ChainHeadExtendedEvent>(o => o.WithHandler<ChainHeadExtendedEventsHandler>())
                .Handle<BlockRolledBackEvent>(o => o.WithHandler<BlockRolledBackEventsHandler>())
                .Handle<LastIrreversibleBlockUpdatedEvent>(o => o.WithHandler<LastIrreversibleBlockUpdatedEventsHandler>())
                .Handle<TransactionExecutedEvent>(o => o.WithHandler<TransactionExecutedEventsHandler>())
                .Handle<TransactionFailedEvent>(o => o.WithHandler<TransactionFailedEventsHandler>());

            _endpoint.Subscribe(
                Bil2IndexerContractExchanges.Events,
                "bil-v2.indexer-grpc-api",
                eventsSubscriptions);
        }
    }
}
