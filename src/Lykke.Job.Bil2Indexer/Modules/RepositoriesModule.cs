using Autofac;
using JetBrains.Annotations;
using Lykke.Job.Bil2Indexer.AzureRepositories;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.Modules
{
    [UsedImplicitly]
    public class RepositoriesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<InMemoryBalanceActionsRepository>().As<IBalanceActionsRepository>().SingleInstance();
            builder.RegisterType<InMemoryBlockHeadersRepository>().As<IBlockHeadersRepository>().SingleInstance();
            builder.RegisterType<InMemoryCoinsRepository>().As<ICoinsRepository>().SingleInstance();
            builder.RegisterType<InMemoryCrawlersRepository>().As<ICrawlersRepository>().SingleInstance();
            builder.RegisterType<InMemoryTransactionsRepository>().As<ITransactionsRepository>().SingleInstance();
            builder.RegisterType<InMemoryChainHeadsRepository>().As<IChainHeadsRepository>().SingleInstance();
        }
    }
}
