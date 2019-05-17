using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.InMemoryRepositories
{
    public class InMemoryAssetInfosRepository : IAssetInfosRepository
    {
        public Task AddIfNotExistsAsync(IReadOnlyCollection<AssetInfo> assetInfos)
        {
            return Task.CompletedTask;
        }

        public Task<AssetInfo> GetOrDefaultAsync(string blockchainType, Asset asset)
        {
            throw new System.NotImplementedException();
        }

        public Task<AssetInfo> GetAsync(string blockchainType, Asset asset)
        {
            throw new System.NotImplementedException();
        }

        public Task<IReadOnlyCollection<AssetInfo>> GetSomeOfAsync(string blockchainType, IEnumerable<Asset> assets)
        {
            throw new System.NotImplementedException();
        }

        public Task<PaginatedItems<AssetInfo>> GetAllAsync(string blockchainType, int limit, string continuation)
        {
            throw new System.NotImplementedException();
        }

        public Task<IReadOnlyCollection<AssetInfo>> GetAllAsync(string blockchainType, int limit, bool orderAsc, string startingAfter = null,
            string endingBefore = null)
        {
            throw new System.NotImplementedException();
        }
    }
}
