using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using JetBrains.Annotations;
using Lykke.Bil2.Client.BlocksReader;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Common.Chaos;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;
using Lykke.Job.Bil2Indexer.DomainServices;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Settings;
using Lykke.Job.Bil2Indexer.Workflow.EventHandlers;
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

            builder.RegisterType<BlockEventsHandler>()
                .As<IBlockEventsHandler>();

            builder.Register(c =>
                {
                    // TODO: Validate, that ranges are not crossed

                    var blocksReaderApiFactory = c.Resolve<IBlocksReaderApiFactory>();
                    var crawlers = _settings.BlockchainIntegrations
                        .ToDictionary(
                            i => i.Type,
                            i => (IReadOnlyCollection<IChainCrawler>) i.Indexer.ChainCrawlers
                                .Select(cs => new ChainCrawler
                                (
                                    i.Type,
                                    cs.StartBlock,
                                    cs.StopBlock,
                                    c.Resolve<IChaosKitty>(),
                                    c.Resolve<IContractEventsPublisher>(),
                                    blocksReaderApiFactory.Create(i.Type),
                                    c.Resolve<IBlockHeadersRepository>(),
                                    c.Resolve<IBlockExpectationRepository>(),
                                    c.Resolve<IBlocksDeduplicationRepository>()
                                ))
                                .ToArray());

                    return new ChainCrawlersManager(crawlers);
                })
                .As<IChainCrawlersManager>()
                .SingleInstance();

            builder.Register(c => new IntegrationSettingsProvider(_settings.BlockchainIntegrations))
                .AsSelf()
                .SingleInstance();
        }
    }
}
