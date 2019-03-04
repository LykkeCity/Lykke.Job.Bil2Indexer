using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Services
{
    public interface IChainCrawler
    {
        Task StartAsync();
        Task ProcessBlockAsync(BlockHeader block);
    }
}
