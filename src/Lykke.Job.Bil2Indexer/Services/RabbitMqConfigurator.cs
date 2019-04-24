using JetBrains.Annotations;
using Lykke.Bil2.RabbitMq;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Bil2.RabbitMq.Subscription.MessageFilters;
using Lykke.Job.Bil2Indexer.Contract;
using Lykke.Job.Bil2Indexer.Contract.Events;
using Lykke.Job.Bil2Indexer.Settings.JobSettings;
using Lykke.Job.Bil2Indexer.Workflow.CommandHandlers;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Job.Bil2Indexer.Workflow.EventHandlers;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Services
{
    [UsedImplicitly]
    public class RabbitMqConfigurator
    {
        public const string CommandsExchangeName = "bil-v2.indexer.commands";

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
            ConfigureCommands();
            ConfigureEvents();
        }

        private void ConfigureEvents()
        {
            _endpoint.DeclareExchange(Bil2IndexerContractExchanges.Events);

            var eventsSubscriptions = new MessageSubscriptionsRegistry()
                .Handle<BlockAssembledEvent>(o => { o.WithHandler<BlockAssembledEventsHandler>(); })
                .Handle<BlockExecutedEvent>(o => o.WithHandler<BlockExecutedEventsHandler>())
                .Handle<CrawlerMovedEvent>(o => o.WithHandler<CrawlerMovedEventsHandler>())
                .Handle<ChainHeadExtendedEvent>(o => o.WithHandler<ChainHeadExtendedEventsHandler>())
                .Handle<ChainHeadReducedEvent>(o => o.WithHandler<ChainHeadReducedEventsHandler>())
                .AddFilter(new AppInsightTelemetryMessageFilter());

            _endpoint.Subscribe(
                eventsSubscriptions,
                Bil2IndexerContractExchanges.Events,
                "bil-v2.indexer",
                _settings.DefaultFirstLevelRetryTimeout,
                _settings.MaxFirstLevelRetryMessageAge,
                _settings.MaxFirstLevelRetryCount,
                _settings.FirstLevelRetryQueueCapacity,
                _settings.ProcessingQueueCapacity,
                _settings.MessageConsumersCount,
                _settings.MessageProcessorsCount,
                CommandsExchangeName);
        }

        private void ConfigureCommands()
        {
            _endpoint.DeclareExchange(CommandsExchangeName);

            var commandsSubscriptions = new MessageSubscriptionsRegistry()
                .Handle<MoveCrawlerCommand>(o => { o.WithHandler<MoveCrawlerCommandsHandler>(); })
                .Handle<RollbackBlockCommand>(o => { o.WithHandler<RollbackBlockCommandsHandler>(); })
                .Handle<WaitForBlockAssemblingCommand>(o => { o.WithHandler<WaitForBlockAssemblingCommandsHandler>(); })
                .Handle<ExecuteTransferCoinsBlockCommand>(o => { o.WithHandler<ExecuteTransferCoinsBlockCommandsHandler>(); })
                .Handle<ExtendChainHeadCommand>(o => { o.WithHandler<ExtendChainHeadCommandsHandler>(); })
                .Handle<ReduceChainHeadCommand>(o => { o.WithHandler<ReduceChainHeadCommandsHandler>(); })
                .AddFilter(new AppInsightTelemetryMessageFilter());
            
            _endpoint.Subscribe(
                commandsSubscriptions,
                CommandsExchangeName,
                "bil-v2.indexer",
                _settings.DefaultFirstLevelRetryTimeout,
                _settings.MaxFirstLevelRetryMessageAge,
                _settings.MaxFirstLevelRetryCount,
                _settings.FirstLevelRetryQueueCapacity,
                _settings.ProcessingQueueCapacity,
                _settings.MessageConsumersCount,
                _settings.MessageProcessorsCount,
                Bil2IndexerContractExchanges.Events);
        }
    }
}
