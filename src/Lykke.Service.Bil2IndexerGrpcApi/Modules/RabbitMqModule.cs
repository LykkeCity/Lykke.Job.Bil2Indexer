using System;
using Autofac;
using JetBrains.Annotations;
using Lykke.Bil2.RabbitMq;
using Lykke.Common;
using Lykke.Service.Bil2IndexerGrpcApi.EventHandlers;
using Lykke.Service.Bil2IndexerGrpcApi.Services;
using Lykke.Service.Bil2IndexerGrpcApi.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.Bil2IndexerGrpcApi.Modules
{
    [UsedImplicitly]
    public class RabbitMqModule : Module
    {
        private readonly AppSettings _settings;

        public RabbitMqModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings.CurrentValue;
        }

        protected override void Load(ContainerBuilder builder)
        {
#if DEBUG
            var vhost = _settings.Bil2IndexerGrpcApi.RabbitMq.Vhost == "/"
                ? null
                : _settings.Bil2IndexerGrpcApi.RabbitMq.Vhost ?? AppEnvironment.EnvInfo;
#else
            var vhost = _settings.Bil2IndexerGrpcApi.RabbitMq.Vhost;
#endif

            builder.RegisterType<RabbitMqEndpoint>()
                .As<IRabbitMqEndpoint>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(new Uri(_settings.Bil2IndexerGrpcApi.RabbitMq.ConnString)))
                .WithParameter(TypedParameter.From(vhost));

            builder.RegisterType<RabbitMqConfigurator>()
                .WithParameter(TypedParameter.From(_settings.Bil2IndexerGrpcApi.RabbitMq))
                .AsSelf();
           
            builder.RegisterType<BlockRolledBackEventsHandler>().AsSelf();
            builder.RegisterType<ChainHeadExtendedEventsHandler>().AsSelf();
            builder.RegisterType<LastIrreversibleBlockUpdatedEventsHandler>().AsSelf();
            builder.RegisterType<TransactionsBatchEventsHandler>().AsSelf();
        }
    }
}
