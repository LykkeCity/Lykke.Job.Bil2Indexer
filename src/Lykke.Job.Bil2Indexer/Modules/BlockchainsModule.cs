using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Lykke.Bil2.Client.BlocksReader;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;
using Lykke.Job.Bil2Indexer.DomainServices;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Settings;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Job.Bil2Indexer.Modules
{
    [UsedImplicitly]
    public class BlockchainsModule : Module
    {
        private readonly AppSettings _settings;

        public BlockchainsModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings.CurrentValue;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var services = new ServiceCollection();

            services.AddBlocksReaderClient(options =>
            {
                options.RabbitMqConnString = _settings.Bil2IndexerJob.RabbitMq.ConnString;
                options.MessageListeningParallelism = _settings.Bil2IndexerJob.RabbitMq.ListeningParallelism;
                options.BlockEventsHandlerFactory = c => c.GetRequiredService<IBlockEventsHandler>();

                foreach (var integration in _settings.BlockchainIntegrations)
                {
                    options.AddIntegration(integration.Type);
                }
            });
            
            builder.Populate(services);
            
            builder.Register(c =>
                {
                    // TODO: Validate, that ranges are not crossed and have no gaps

                    var crawlers = _settings.BlockchainIntegrations
                        .ToDictionary(
                            i => i.Type,
                            i => (IReadOnlyCollection<CrawlerConfiguration>) i.Indexer.ChainCrawlers
                                .Select(cs => new CrawlerConfiguration(cs.StartBlock, cs.StopAssemblingBlock))
                                .ToArray());

                    return new CrawlersManager(
                        c.Resolve<ICrawlersRepository>(),
                        c.Resolve<IBlocksReaderApiFactory>(),
                        crawlers);
                })
                .As<ICrawlersManager>()
                .SingleInstance();

            builder.Register(c =>
                {
                    var firstBlockNumbers = _settings.BlockchainIntegrations.ToDictionary(i => i.Type, i => i.Capabilities.FirstBlockNumber);

                    return new ChainHeadsManager(c.Resolve<IChainHeadsRepository>(), firstBlockNumbers);
                })
                .As<IChainHeadsManager>()
                .SingleInstance();

            builder.Register(c => new IntegrationSettingsProvider(_settings.BlockchainIntegrations))
                .AsSelf()
                .SingleInstance();
        }
    }
}
