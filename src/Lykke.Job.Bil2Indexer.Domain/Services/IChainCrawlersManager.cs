using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Services
{
    public interface IChainCrawlersManager
    {
        Task StartAsync();
        Task ProcessBlockAsync(string blockchainName, BlockHeader block);
    }
}
