using JetBrains.Annotations;
using Lykke.Bil2.RabbitMq;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Common;
using Lykke.Job.Bil2Indexer.Contract;
using Lykke.Job.Bil2Indexer.Contract.Events;
using Lykke.Service.Bil2IndexerGrpcApi.EventHandlers;
using Lykke.Service.Bil2IndexerGrpcApi.Settings;

namespace Lykke.Service.Bil2IndexerGrpcApi.Services
{
    [UsedImplicitly]
    public class RabbitMqConfigurator
    {
        private readonly IRabbitMqEndpoint _endpoint;
        private readonly RabbitMqSettings _settings;

        public RabbitMqConfigurator(
            IRabbitMqEndpoint endpoint,
            RabbitMqSettings settings)
        {
            _endpoint = endpoint;
            _settings = settings;
        }

        public void Configure()
        {
            _endpoint.DeclareExchange(Bil2IndexerContractExchanges.Events);

            var eventsSubscriptions = new MessageSubscriptionsRegistry()
                .Handle<ChainHeadExtendedEvent>(o => o.WithHandler<ChainHeadExtendedEventsHandler>())
                .Handle<ChainHeadReducedEvent>(o => o.WithHandler<ChainHeadReducedEventsHandler>())
                .Handle<LastIrreversibleBlockUpdatedEvent>(o => o.WithHandler<LastIrreversibleBlockUpdatedEventsHandler>())
                .Handle<TransactionExecutedEvent>(o => o.WithHandler<TransactionExecutedEventsHandler>())
                .Handle<TransactionFailedEvent>(o => o.WithHandler<TransactionFailedEventsHandler>());

            _endpoint.Subscribe
            (
                eventsSubscriptions,
                Bil2IndexerContractExchanges.Events,
                "bil-v2.indexer-grpc-api",
                _settings.DefaultFirstLevelRetryTimeout,
                _settings.MaxFirstLevelRetryMessageAge,
                _settings.MaxFirstLevelRetryCount,
                _settings.FirstLevelRetryQueueCapacity,
                _settings.ProcessingQueueCapacity,
                _settings.MessageConsumersCount,
                _settings.MessageProcessorsCount
            );
        }
    }
}
