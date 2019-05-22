using Autofac;
using Lykke.Job.Bil2Indexer.Domain.Services;
using Lykke.Job.Bil2Indexer.Domain.Services.Infrastructure;
using Lykke.Job.Bil2Indexer.DomainServices;
using Lykke.Job.Bil2Indexer.DomainServices.Infrastructure;
using Lykke.Service.Bil2IndexerWebApi.Services;
using Lykke.Service.Bil2IndexerWebApi.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.Bil2IndexerWebApi.Modules
{
    public class ServiceModule : Module
    {
        private readonly AppSettings _settings;

        public ServiceModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings.CurrentValue;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<AppInsightTelemetryProvider>()
                .As<IAppInsightTelemetryProvider>();

            builder.RegisterType<AssetInfosManager>()
                .As<IAssetInfosManager>()
                .As<IAssetInfosProvider>()
                .WithParameter(TypedParameter.From(_settings.Bil2WebApiService.AssetsCaching.LruCacheCapacity))
                .SingleInstance();
            
            builder.RegisterType<AddressQueryFacade>()
                .As<IAddressQueryFacade>();

            builder.RegisterType<AssetQueryFacade>()
                .As<IAssetQueryFacade>();
            
            builder.RegisterType<BlockQueryFacade>()
                .As<IBlockQueryFacade>();
            
            builder.RegisterType<TransactionQueryFacade>()
                .As<ITransactionQueryFacade>();
        }
    }
}
