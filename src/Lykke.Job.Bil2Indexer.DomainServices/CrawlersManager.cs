using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Bil2.Contract.BlocksReader.Commands;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;

namespace Lykke.Job.Bil2Indexer.DomainServices
{
    public class CrawlersManager : ICrawlersManager
    {
        private readonly ICrawlersRepository _crawlersRepository;
        private readonly IBlocksReaderApiFactory _blocksReaderApiFactory;
        private readonly IReadOnlyDictionary<string, IReadOnlyCollection<CrawlerConfiguration>> _crawlerConfigurations;

        public CrawlersManager(
            ICrawlersRepository crawlersRepository,
            IBlocksReaderApiFactory blocksReaderApiFactory,
            IReadOnlyDictionary<string, IReadOnlyCollection<CrawlerConfiguration>> crawlerConfigurations)
        {
            _crawlersRepository = crawlersRepository;
            _blocksReaderApiFactory = blocksReaderApiFactory;
            _crawlerConfigurations = crawlerConfigurations;
        }

        public async Task StartAsync()
        {
            foreach (var (blockchainType, crawlerConfigurations) in _crawlerConfigurations)
            {
                foreach (var crawlerConfiguration in crawlerConfigurations)
                {
                    await StartCrawlerAsync(blockchainType, crawlerConfiguration);
                }
            }
        }

        private async Task StartCrawlerAsync(string blockchainType, CrawlerConfiguration crawlerConfiguration)
        {
            var crawler = await _crawlersRepository.GetOrDefaultAsync
            (
                blockchainType,
                crawlerConfiguration
            );

            if (crawler == null)
            {
                crawler = Crawler.StartNew(blockchainType, crawlerConfiguration);

                await _crawlersRepository.SaveAsync(crawler);
            }

            if (crawler.Configuration.CanProcess(crawler.ExpectedBlockNumber))
            {
                var blocksReaderApi = _blocksReaderApiFactory.Create(blockchainType);
                
                await blocksReaderApi.SendAsync(new ReadBlockCommand(crawler.ExpectedBlockNumber), crawler.GetCorrelationId().ToString());
            }
        }

        public Task<Crawler> GetCrawlerAsync(string blockchainType, long blockNumber)
        {
            var crawlerConfigurations = _crawlerConfigurations[blockchainType];
            var crawlerConfiguration = crawlerConfigurations.SingleOrDefault(c => c.CanProcess(blockNumber));

            if (crawlerConfiguration == null)
            {
                throw new InvalidOperationException($"No crawler configured to process block: {blockchainType}:{blockNumber}");
            }

            return GetCrawlerAsync(blockchainType, crawlerConfiguration);
        }

        public async Task<Crawler> GetCrawlerAsync(string blockchainType, CrawlerConfiguration configuration)
        {
            var crawler = await _crawlersRepository.GetOrDefaultAsync(blockchainType, configuration);

            if (crawler == null)
            {
                throw new InvalidOperationException($"No crawler found: {blockchainType}:{configuration}");
            }

            return crawler;
        }
    }
}
