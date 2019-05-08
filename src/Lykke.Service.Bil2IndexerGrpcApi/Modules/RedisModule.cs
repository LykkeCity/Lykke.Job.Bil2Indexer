using Autofac;
using JetBrains.Annotations;
using Lykke.Service.Bil2IndexerGrpcApi.Settings;
using Lykke.SettingsReader;
using StackExchange.Redis;

namespace Lykke.Service.Bil2IndexerGrpcApi.Modules
{
    [UsedImplicitly]
    public class RedisModule : Module
    {
        private readonly AppSettings _settings;

        public RedisModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings.CurrentValue;
        }

        protected override void Load(ContainerBuilder builder)
        {
            System.Threading.ThreadPool.SetMinThreads(100, 100);
            var options = ConfigurationOptions.Parse(_settings.Bil2IndexerGrpcApi.CacheSettings.RedisConfiguration);
            options.ReconnectRetryPolicy = new ExponentialRetry(3000, 15000);
            options.ClientName = "Lykke.Service.Bil2IndexerGrpcApi";

            var redis = ConnectionMultiplexer.Connect(options);

            builder.RegisterInstance(redis).SingleInstance();
        }
    }
}
