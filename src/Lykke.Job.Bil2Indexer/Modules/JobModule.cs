using Autofac;
using Hangfire;
using Hangfire.Autofac;
using Hangfire.MemoryStorage;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Workflow.BackgroundJobs;
using Lykke.Logs.Hangfire;
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

            builder.RegisterType<RetryNotFoundBlockJob>()
                .AsSelf();

            builder
                .RegisterBuildCallback(StartHangfireServer)
                .Register(c => new BackgroundJobServer())
                .SingleInstance();
        }

        private static void StartHangfireServer(IContainer container)
        {
            GlobalConfiguration.Configuration
                .UseMemoryStorage();

            GlobalConfiguration.Configuration
                .UseLykkeLogProvider(container)
                .UseAutofacActivator(container);

            container.Resolve<BackgroundJobServer>();
        }
    }
}
