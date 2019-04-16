using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain.Services
{
    public interface IAssetInfosProvider
    {
        Task<AssetInfo> GetAsync(string blockchainType, AssetId id);
        Task<IReadOnlyDictionary<AssetId, int>> GetSomeOfAsync(string blockchainType, IEnumerable<AssetId> ids);
        Task<IReadOnlyDictionary<AssetId, int>> GetAllOfAsync(string blockchainType, IEnumerable<AssetId> ids);
    }
}

