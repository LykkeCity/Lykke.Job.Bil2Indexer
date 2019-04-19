using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain.Services
{
    public interface IAssetInfosProvider
    {
        Task<AssetInfo> GetAsync(string blockchainType, Asset asset);
        Task<IReadOnlyCollection<AssetInfo>> GetSomeOfAsync(string blockchainType, IEnumerable<Asset> assets);
    }
}

