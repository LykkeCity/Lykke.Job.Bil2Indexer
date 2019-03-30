using Autofac;
using JetBrains.Annotations;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Workflow.CommandHandlers;
using Lykke.Job.Bil2Indexer.Workflow.EventHandlers;

namespace Lykke.Job.Bil2Indexer.Modules
{
    [UsedImplicitly]
    public class RabbitMqModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RabbitMqConfigurator>()
                .AsSelf();

            builder.RegisterType<CommandsSenderFactory>()
                .As<ICommandsSenderFactory>();

            
            builder.RegisterType<BlockReaderEventsHandler>().As<IBlockEventsHandler>();
            builder.RegisterType<BlockBuildingEventsHandler>().AsSelf();
            builder.RegisterType<CrawlingEventsHandler>().AsSelf();

            builder.RegisterType<BlockBuildingCommandsHandler>().AsSelf();
            builder.RegisterType<CrawlingCommandsHandler>().AsSelf();
        }
    }
}
