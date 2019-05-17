using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface IBlockHeadersRepository
    {
        Task SaveAsync(BlockHeader block);
        Task<BlockHeader> GetOrDefaultAsync(string blockchainType, long blockNumber);
        Task<BlockHeader> GetOrDefaultAsync(string blockchainType, BlockId blockId);
        Task<BlockHeader> GetAsync(string blockchainType, long blockNumber);
        Task<BlockHeader> GetAsync(string blockchainType, BlockId blockId);
        Task TryRemoveAsync(string blockchainType, BlockId blockId);
    }
}
