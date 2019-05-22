using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Services
{
    public interface ICrawlersManager
    {
        Task StartAsync();
        Task<Crawler> GetCrawlerAsync(string blockchainType, long blockNumber);
        Task<Crawler> GetCrawlerAsync(string blockchainType, CrawlerConfiguration configuration);
        Task<Crawler> GetInfiniteCrawlerAsync(string blockchainType);
        CrawlerConfiguration GetInfiniteCrawlerConfiguration(string blockchainType);
    }
}
