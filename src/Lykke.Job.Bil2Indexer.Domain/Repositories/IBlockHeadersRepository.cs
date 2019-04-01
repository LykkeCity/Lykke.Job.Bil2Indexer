using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface IBlockHeadersRepository
    {
        Task SaveAsync(BlockHeader block);
        Task<BlockHeader> GetOrDefaultAsync(string blockchainType, long blockNumber);
        Task<BlockHeader> GetOrDefaultAsync(string blockchainType, string blockId);
        Task<BlockHeader> GetAsync(string blockchainType, string blockId);
    }
}
