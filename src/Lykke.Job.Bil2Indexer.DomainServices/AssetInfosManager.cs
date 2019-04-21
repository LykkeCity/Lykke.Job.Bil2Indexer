using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluidCaching;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Infrastructure;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;

namespace Lykke.Job.Bil2Indexer.DomainServices
{
    public class AssetInfosManager :
        IAssetInfosManager,
        IAssetInfosProvider
    {
        private readonly IAssetInfosRepository _repository;
        private readonly FluidCache<AssetInfo> _cache;
        private readonly IIndex<(string BlockchainType, Asset Asset), AssetInfo> _indexByFullId;


        public AssetInfosManager(
            IAssetInfosRepository repository,
            int cacheCapacity)
        {
            _repository = repository;
            _cache = new FluidCache<AssetInfo>(cacheCapacity, TimeSpan.Zero, TimeSpan.MaxValue, () => DateTime.UtcNow);
            _indexByFullId = _cache.AddIndex
            (
                "byFullId",
                x => (x.BlockchainType, x.Asset)
            );
        }

        public async Task EnsureAdded(ISet<AssetInfo> assets)
        {
            var notCachedAssets = assets
                .Where(x => _indexByFullId.GetItem((x.BlockchainType, x.Asset)) != null)
                .ToArray();
            
            await _repository.AddIfNotExistsAsync(notCachedAssets);

            foreach (var asset in notCachedAssets)
            {
                _cache.Add(asset);
            }
        }

        public async Task<AssetInfo> GetAsync(string blockchainType, Asset asset)
        {
            var assetInfo = await _indexByFullId.GetItem
            (
                (blockchainType, asset),
                key => _repository.GetOrDefaultAsync(key.BlockchainType, key.Asset)
            );

            if (assetInfo == null)
            {
                throw new InvalidOperationException($"Asset not found {blockchainType}:{asset}");
            }

            return assetInfo;
        }

        public async Task<IReadOnlyCollection<AssetInfo>> GetSomeOfAsync(string blockchainType, IEnumerable<Asset> assets)
        {
            var results = await assets.MapAsync
            (
                degreeOfParallelism: 8,
                body: x => GetAsync(blockchainType, x)
            );

            return results.Where(x => x != null).ToArray();
        }
    }
}
