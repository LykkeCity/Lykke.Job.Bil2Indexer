using JetBrains.Annotations;
using Lykke.Bil2.RabbitMq;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Contract;
using Lykke.Job.Bil2Indexer.Contract.Events;
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

        public RabbitMqConfigurator(IRabbitMqEndpoint endpoint)
        {
            _endpoint = endpoint;
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
                .Handle<BlockExecutedEvent>(o => o.WithHandler<BlockExecutionEventsHandler>())
                .Handle<BlockPartiallyExecutedEvent>(o => o.WithHandler<BlockExecutionEventsHandler>())
                .Handle<CrawlerMovedEvent>(o => o.WithHandler<CrawlerMovedEventsHandler>())
                .Handle<ChainHeadExtendedEvent>(o => o.WithHandler<ChainHeadExtendedEventsHandler>());

            _endpoint.Subscribe(
                Bil2IndexerContractExchanges.Events,
                "bil-v2.indexer",
                eventsSubscriptions,
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
                .Handle<ExtendChainHeadCommand>(o => { o.WithHandler<ExtendChainHeadCommandsHandler>(); });
            
            _endpoint.Subscribe(
                CommandsExchangeName,
                "bil-v2.indexer",
                commandsSubscriptions,
                Bil2IndexerContractExchanges.Events);
        }
    }
}
