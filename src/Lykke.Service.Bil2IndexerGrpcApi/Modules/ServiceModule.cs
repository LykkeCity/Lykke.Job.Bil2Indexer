using Autofac;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.Bil2IndexerGrpcApi.Services;

namespace Lykke.Service.Bil2IndexerGrpcApi.Modules
{
    [UsedImplicitly]
    public class ServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .AutoActivate()
                .SingleInstance();
        }
    }
}
