using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public interface IAssetQueryFacade
    {
        Task<AssetModel> GetAsset(string blockchainType, string address, string ticker);
        Task<IReadOnlyCollection<AssetModel>> GetAssets(string blockchainType, int limit, bool orderAsc, string startingAfter = null, string endingBefore = null);
    }
}
