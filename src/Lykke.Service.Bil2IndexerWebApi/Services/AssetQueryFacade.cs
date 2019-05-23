using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Service.Bil2IndexerWebApi.Mappers;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public class AssetQueryFacade: IAssetQueryFacade
    {
        private readonly IAssetInfosRepository _assetInfosRepository;

        public AssetQueryFacade(IAssetInfosRepository assetInfosRepository)
        {
            _assetInfosRepository = assetInfosRepository;
        }

        public async Task<AssetModel> GetAsset(string blockchainType, string ticker, string address)
        {
            return (await _assetInfosRepository.GetOrDefaultAsync(blockchainType,
                    new Asset(new AssetId(ticker), ticker != null ? new AssetAddress(address) : null)))
                ?.ToViewModel();
        }

        public async Task<IReadOnlyCollection<AssetModel>> GetAssets(string blockchainType, int limit, bool orderAsc, string startingAfter = null, string endingBefore = null)
        {
            return (await _assetInfosRepository.GetCollectionAsync(blockchainType, 
                    limit,
                    orderAsc,
                    AssetIdBuilder.BuildDomainOrDefault(startingAfter),
                    AssetIdBuilder.BuildDomainOrDefault(endingBefore)))
                .ToViewModel();
        }
    }
}
