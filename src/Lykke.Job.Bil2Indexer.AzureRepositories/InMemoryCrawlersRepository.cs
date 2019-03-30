using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.AzureRepositories
{
    public class InMemoryCrawlersRepository : ICrawlersRepository
    {
        private readonly ConcurrentDictionary<(string, CrawlerConfiguration), Crawler> _storage;

        public InMemoryCrawlersRepository()
        {
            _storage = new ConcurrentDictionary<(string, CrawlerConfiguration), Crawler>();
        }

        public Task<Crawler> GetOrDefaultAsync(string blockchainType, CrawlerConfiguration configuration)
        {
            _storage.TryGetValue((blockchainType, configuration), out var crawler);

            return Task.FromResult(crawler);
        }

        public Task SaveAsync(Crawler crawler)
        {
            _storage.AddOrUpdate
            (
                (crawler.BlockchainType, crawler.Configuration),
                id => crawler,
                (id, oldCrawler) =>
                {
                    if (oldCrawler.Version != crawler.Version)
                    {
                        throw new InvalidOperationException($"Optimistic concurrency: crawler versions mismatch. Expected version {oldCrawler.Version}, actual {crawler.Version}");
                    }

                    var newCrawler = new Crawler
                    (
                        crawler.BlockchainType,
                        crawler.Version + 1,
                        crawler.Sequence,
                        crawler.Configuration,
                        crawler.ExpectedBlockNumber
                    );

                    Console.WriteLine($"Crawler saved: {newCrawler}");

                    return newCrawler;
                }
            );

            return Task.CompletedTask;
        }
    }
}
