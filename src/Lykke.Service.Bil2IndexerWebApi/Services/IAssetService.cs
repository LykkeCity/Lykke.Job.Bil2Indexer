using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public interface IAssetService
    {
        Task<AssetInfo> GetAsset(string blockchainType, string address, string ticker);
        Task<IReadOnlyCollection<AssetInfo>> GetAssets(string blockchainType, int limit, bool orderAsc, string startingAfter, string endingBefore);
    }
}
