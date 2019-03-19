using Autofac;
using Lykke.Job.Bil2Indexer.AzureRepositories;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.Modules
{
    public class RepositoriesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<InMemoryBlockHeadersRepository>().As<IBlockHeadersRepository>().SingleInstance();
            builder.RegisterType<InMemoryBlockExpectationRepository>().As<IBlockExpectationRepository>().SingleInstance();
            builder.RegisterType<InMemoryBlocksDeduplicationRepository>().As<IBlocksDeduplicationRepository>().SingleInstance();
            builder.RegisterType<InMemoryBlockBuildingRepository>().As<IBlockBuildingsRepository>().SingleInstance();
        }
    }
}
