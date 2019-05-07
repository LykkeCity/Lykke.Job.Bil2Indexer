using Autofac;
using Bil2.Indexer;
using Grpc.Core;
using JetBrains.Annotations;
using Lykke.Service.Bil2IndexerGrpcApi.Services;
using Lykke.Service.Bil2IndexerGrpcApi.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.Bil2IndexerGrpcApi.Modules
{
    [UsedImplicitly]
    public class GrpcModule : Module
    {
        private readonly AppSettings _settings;

        public GrpcModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings.CurrentValue;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<IndexerApiImpl>()
                .AsSelf()
                .SingleInstance();

            var host = "0.0.0.0";
            builder.Register(ctx =>
                new Server
                {
                    Services = { IndexerApi.BindService(ctx.Resolve<IndexerApiImpl>()) },
                    Ports = { new ServerPort(host, _settings.Bil2IndexerGrpcApi.GrpcSettings.Port, ServerCredentials.Insecure) }
                }
            ).SingleInstance();
        }
    }
}
