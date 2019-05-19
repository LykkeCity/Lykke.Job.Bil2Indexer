using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface IAssetInfosRepository
    {
        Task AddIfNotExistsAsync(IReadOnlyCollection<AssetInfo> assetInfos);
        Task<AssetInfo> GetOrDefaultAsync(string blockchainType, Asset asset);
        Task<AssetInfo> GetAsync(string blockchainType, Asset asset);
        Task<IReadOnlyCollection<AssetInfo>> GetSomeOfAsync(string blockchainType, IEnumerable<Asset> assets);
        Task<PaginatedItems<AssetInfo>> GetPagedAsync(string blockchainType, int limit, string continuation);
        Task<IReadOnlyCollection<AssetInfo>> GetCollectionAsync(string blockchainType, int limit, bool orderAsc, string startingAfter = null, string endingBefore = null);
    }
}
