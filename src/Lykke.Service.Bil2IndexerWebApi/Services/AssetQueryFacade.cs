using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Service.Bil2IndexerWebApi.Mappers;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public class AssetQueryFacade: IAssetQueryFacade
    {
        private readonly IAssetInfosRepository _assetInfosRepository;

        public AssetQueryFacade(IAssetInfosRepository assetInfosRepository)
        {
            _assetInfosRepository = assetInfosRepository;
        }

        public async Task<AssetModel> GetAsset(string blockchainType, string address, string ticker)
        {
            return (await _assetInfosRepository.GetOrDefaultAsync(blockchainType,
                new Asset(new AssetId(ticker), ticker != null ? new AssetAddress(ticker) : null)))?.ToViewModel();
        }

        public async Task<Paginated<AssetModel>> GetAssets(string blockchainType, int limit, bool orderAsc, string startingAfter = null, string endingBefore = null)
        {
            return (await _assetInfosRepository.GetAllAsync(blockchainType, limit, orderAsc, startingAfter, endingBefore)).ToViewModel();
        }
    }
}
