using Autofac;
using Lykke.Service.Bil2IndexerWebApi.Factories;

namespace Lykke.Service.Bil2IndexerWebApi.Modules
{
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //ToDo: register all services / factories from assembly, not 1 by 1
            services.AddSingleton<IAddressService, AddressService>();

            services.AddSingleton<IBlockService, BlockService>();

            services.AddSingleton<IAddressService, AddressService>();

            services.AddSingleton<IAssetService, AssetService>();

            services.AddSingleton<ITransactionService, TransactionService>();

            services.AddSingleton<IAssetModelFactory, AssetModelFactory>();

            services.AddSingleton<IBlockModelFactory, BlockModelFactory>();

            services.AddSingleton<ITransactionModelFactory, TransactionModelFactory>();
        }
    }
}
