using Autofac;
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
            builder.Register(ctx => new IndexerApiImpl())
                .SingleInstance();

            const int Port = 50051;
            builder.Register(ctx =>
                new Server
                {
                    Services = { IndexerApi.BindService(ctx.Resolve<IndexerApiImpl>()) },
                    Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
                }
            ).SingleInstance();
        }
    }
}
