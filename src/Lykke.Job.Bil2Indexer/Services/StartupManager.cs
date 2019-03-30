using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Domain.Services;
using Lykke.Sdk;

namespace Lykke.Job.Bil2Indexer.Services
{
    public class StartupManager : IStartupManager
    {
        private readonly RabbitMqConfigurator _rabbitMqConfigurator;
        private readonly IBlocksReaderClient _blocksReaderClient;
        private readonly Func<ICrawlersManager> _chainCrawlersManager;
        private readonly ILog _log;

        public StartupManager(
            ILogFactory logFactory,
            RabbitMqConfigurator rabbitMqConfigurator,
            IBlocksReaderClient blocksReaderClient,
            Func<ICrawlersManager> chainCrawlersManager)
        {
            _rabbitMqConfigurator = rabbitMqConfigurator;
            _blocksReaderClient = blocksReaderClient;
            _chainCrawlersManager = chainCrawlersManager;
            _log = logFactory.CreateLog(this);
        }

        public async Task StartAsync()
        {
            _log.Info("Initializing blocks reader client...");

            _blocksReaderClient.Initialize();
            
            _log.Info("Initializing indexer messaging configuration...");

            _rabbitMqConfigurator.Configure();

            _log.Info("Starting crawlers manager...");

            await _chainCrawlersManager.Invoke().StartAsync();

            _log.Info("Starting events listening...");
            
            _blocksReaderClient.StartListening();
        }
    }
}
