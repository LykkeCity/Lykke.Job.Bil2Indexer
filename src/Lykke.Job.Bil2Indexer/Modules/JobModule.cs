using Autofac;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Job.Bil2Indexer.Domain.Services.Infrastructure;
using Lykke.Job.Bil2Indexer.DomainServices.Infrastructure;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Sdk;

namespace Lykke.Job.Bil2Indexer.Modules
{
    [UsedImplicitly]
    public class JobModule : Module
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

            builder.RegisterChaosKitty(null);

            builder.RegisterType<AppInsightTelemetryProvider>()
                .As<IAppInsightTelemetryProvider>();
        }
    }
}
