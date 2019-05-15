using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public interface IAssetQueryFacade
    {
        Task<AssetModel> GetAsset(string blockchainType, string address, string ticker);
        Task<Paginated<AssetModel>> GetAssets(string blockchainType, int limit, bool orderAsc, string startingAfter = null, string endingBefore = null);
    }
}
