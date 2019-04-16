using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface IAssetInfosRepository
    {
        Task AddIfNotExistsAsync(AssetInfo asset);
        Task<AssetInfo> GetOrDefaultAsync(string blockchainType, AssetId id);
        Task<AssetInfo> GetAsync(string blockchainType, AssetId id);
        Task<IReadOnlyCollection<AssetInfo>> GetSomeOfAsync(string blockchainType, IEnumerable<AssetId> ids);
        Task<PaginatedItems<AssetInfo>> GetAllAsync(string blockchainType, int limit, string continuation);
    }
}
