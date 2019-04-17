using Autofac;
using Common.Application;
using JetBrains.Annotations;
using Lykke.Common;
using Lykke.Sdk;
using Lykke.Service.Bil2IndexerGrpcApi.Services;
using Lykke.Service.Bil2IndexerGrpcApi.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.Bil2IndexerGrpcApi.Modules
{
    [UsedImplicitly]
    public class ServiceModule : Module
    {
        private readonly AppSettings _settings;

        public ServiceModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings.CurrentValue;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .WithParameter(new NamedParameter("rabbitMqListeningParallelism", _settings.Bil2IndexerGrpcApi.RabbitMq.ListeningParallelism))
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .AutoActivate()
                .SingleInstance();
        }
    }
}
