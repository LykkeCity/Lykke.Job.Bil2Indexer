using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface IBlockHeadersRepository
    {
        Task SaveAsync(BlockHeader block);
        Task<BlockHeader> GetOrDefaultAsync(long blockNumber);
        Task RemoveAsync(BlockHeader block);
    }
}
