using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Services;

namespace Lykke.Job.Bil2Indexer.DomainServices
{
    public class ChainCrawlersManager : IChainCrawlersManager
    {
        private readonly IReadOnlyDictionary<string, IReadOnlyCollection<IChainCrawler>> _crawlers;

        public ChainCrawlersManager(IReadOnlyDictionary<string, IReadOnlyCollection<IChainCrawler>> crawlers)
        {
            _crawlers = crawlers;
        }

        public async Task StartAsync()
        {
            foreach (var crawler in _crawlers.Values.SelectMany(x => x))
            {
                await crawler.StartAsync();
            }
        }

        public Task ProcessBlockAsync(string blockchainName, BlockHeader block)
        {
            var crawlers = _crawlers[blockchainName];
            var tasks = crawlers.Select(c => c.ProcessBlockAsync(block));

            return Task.WhenAll(tasks);
        }
    }
}
