using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.RabbitMq;
using Lykke.Common.Log;
using Lykke.Sdk;

namespace Lykke.Service.Bil2IndexerGrpcApi.Services
{
    public class StartupManager : IStartupManager
    {
        private readonly RabbitMqConfigurator _rabbitMqConfigurator;
        private readonly IRabbitMqEndpoint _rabbitMqEndpoint;
        private readonly int _rabbitMqListeningParallelism;
        private readonly ILog _log;

        public StartupManager(
            ILogFactory logFactory,
            RabbitMqConfigurator rabbitMqConfigurator,
            IRabbitMqEndpoint rabbitMqEndpoint,
            int rabbitMqListeningParallelism)
        {
            _rabbitMqConfigurator = rabbitMqConfigurator;
            _rabbitMqEndpoint = rabbitMqEndpoint;
            _rabbitMqListeningParallelism = rabbitMqListeningParallelism;

            _log = logFactory.CreateLog(this);
        }

        public Task StartAsync()
        {
            _log.Info("Initializing RabbitMQ endpoint...");
            
            _rabbitMqEndpoint.Initialize();

            _log.Info("Initializing RabbitMQ messaging configuration...");

            _rabbitMqConfigurator.Configure();

            _log.Info("Starting RabbitMQ endpoint...");

            _rabbitMqEndpoint.StartListening(_rabbitMqListeningParallelism);

            return Task.CompletedTask;
        }
    }
}
