using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public class AssetService: IAssetService
    {
        private readonly IAssetInfosRepository _assetInfosRepository;

        public AssetService(IAssetInfosRepository assetInfosRepository)
        {
            _assetInfosRepository = assetInfosRepository;
        }

        public Task<AssetInfo> GetAsset(string blockchainType, string address, string ticker)
        {
            return _assetInfosRepository.GetOrDefaultAsync(blockchainType,
                new Asset(new AssetId(ticker), ticker != null ? new AssetAddress(ticker) : null));
        }

        public Task<IReadOnlyCollection<AssetInfo>> GetAssets(string blockchainType, int limit, bool orderAsc, string startingAfter, string endingBefore)
        {
            return _assetInfosRepository.GetAllAsync(blockchainType, limit, orderAsc, startingAfter, endingBefore);
        }
    }
}
