using System.Collections.Generic;
using System.Linq;
using Autofac;
using JetBrains.Annotations;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Settings;
using Lykke.Job.Bil2Indexer.Workflow.CommandHandlers;
using Lykke.Job.Bil2Indexer.Workflow.EventHandlers;
using Lykke.SettingsReader;

namespace Lykke.Job.Bil2Indexer.Modules
{
    [UsedImplicitly]
    public class RabbitMqModule : Module
    {
        private readonly AppSettings _settings;

        public RabbitMqModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings.CurrentValue;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RabbitMqConfigurator>()
                .WithParameter(TypedParameter.From(_settings.Bil2IndexerJob.RabbitMq))
                .AsSelf();

            builder.RegisterType<MessageSendersFactory>()
                .As<IMessageSendersFactory>();
            
            builder.RegisterType<BlockReaderEventsHandler>().As<IBlockEventsHandler>();
            builder.RegisterType<BlockAssembledEventsHandler>().AsSelf();
            builder.RegisterType<BlockExecutedEventsHandler>().AsSelf();
            builder.RegisterType<CrawlerMovedEventsHandler>().AsSelf();
            builder.RegisterType<ChainHeadExtendedEventsHandler>().AsSelf();
            builder.RegisterType<ChainHeadReducedEventsHandler>().AsSelf();

            builder.RegisterType<ExecuteTransferCoinsBlockCommandsHandler>().AsSelf();
            builder.RegisterType<MoveCrawlerCommandsHandler>().AsSelf();
            builder.RegisterType<RollbackBlockCommandsHandler>().AsSelf();
            builder.RegisterType<WaitForBlockAssemblingCommandsHandler>()
                .WithParameter(TypedParameter.From(_settings.Bil2IndexerJob.BlocksAssembling.RetryTimeout))
                .AsSelf();

            IReadOnlyDictionary<string, long> blockNumbersToStartTransactionEventsPublication = _settings.Bil2IndexerJob.BlockchainIntegrations
                .ToDictionary(x => x.Type, x => x.Indexer.BlockNumberToStartTransactionEventsPublication);

            builder.RegisterType<ExtendChainHeadCommandsHandler>()
                .WithParameter(TypedParameter.From(blockNumbersToStartTransactionEventsPublication))
                .AsSelf();
            builder.RegisterType<ReduceChainHeadCommandsHandler>().AsSelf();
        }
    }
}
