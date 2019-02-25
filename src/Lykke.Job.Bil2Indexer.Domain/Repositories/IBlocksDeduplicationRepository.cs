using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface IBlocksDeduplicationRepository
    {
        Task<bool> IsProcessedAsync(string blockHash);
        Task MarkAsProcessedAsync(string blockHash);
        Task MarkAsNotProcessedAsync(string blockHash);
    }
}
