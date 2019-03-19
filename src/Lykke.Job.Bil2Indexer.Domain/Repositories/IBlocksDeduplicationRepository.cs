using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface IBlocksDeduplicationRepository
    {
        Task<bool> IsProcessedAsync(string blockId);
        Task MarkAsProcessedAsync(string blockId);
        Task MarkAsNotProcessedAsync(string blockId);
    }
}
