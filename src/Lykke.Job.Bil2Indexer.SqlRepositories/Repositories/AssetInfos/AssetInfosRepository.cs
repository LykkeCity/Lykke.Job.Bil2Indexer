using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.AssetInfos.Mappers;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.AssetInfos
{
    public class AssetInfosRepository: IAssetInfosRepository
    {
        private readonly IPgConnectionStringProvider _connectionStringProvider;

        public AssetInfosRepository(IPgConnectionStringProvider connectionStringProvider)
        {
            _connectionStringProvider = connectionStringProvider;
        }

        public async Task AddIfNotExistsAsync(IReadOnlyCollection<AssetInfo> assets)
        {
            if (!assets.Any())
            {
                return;
            }

            foreach (var asset in assets)
            {
                using (var db =
                    new DataContext(
                        _connectionStringProvider.GetConnectionString(assets.First().BlockchainType)))
                {
                    var dbEntity = asset.ToDbEntity();

                    await db.AssetInfos.AddAsync(dbEntity);

                    try
                    {
                        await db.SaveChangesAsync();
                    }
                    catch (DbUpdateException e) when (e.IsNaturalKeyViolationException())
                    {
                        //assume entity already exist in db
                    }

                }
            }
        }

        public async Task<AssetInfo> GetOrDefaultAsync(string blockchainType, Asset asset)
        {
            using (var db = new DataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var entity = await db.AssetInfos
                    .SingleOrDefaultAsync(AssetInfosPredicates.Build(asset));

                return entity?.ToDomain(blockchainType);
            }
        }

        public async Task<AssetInfo> GetAsync(string blockchainType, Asset asset)
        {
            using (var db = new DataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var entity = await db.AssetInfos
                    .SingleAsync(AssetInfosPredicates.Build(asset));

                return entity.ToDomain(blockchainType);
            }
        }

        public async Task<IReadOnlyCollection<AssetInfo>> GetSomeOfAsync(string blockchainType, IEnumerable<Asset> assets)
        {
            var ids = assets.Select(AssetIdBuilder.BuildId).ToList();

            using (var db = new DataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var entities = await db.AssetInfos
                    .Where(p => ids.Contains(p.Id))
                    .ToListAsync();

                return entities.Select(p => p.ToDomain(blockchainType)).ToList();
            }
        }

        public async Task<PaginatedItems<AssetInfo>> GetPagedAsync(string blockchainType, int limit, string continuation)
        {
            using (var db = new DataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                int skip = 0;
                if (!string.IsNullOrEmpty(continuation))
                {
                    skip = int.Parse(continuation);
                }

                var entities = await db.AssetInfos
                    .Skip(skip)
                    .Take(limit)
                    .ToListAsync();

                var nextContinuation = entities.Count < limit ? null : (skip + entities.Count).ToString();

                return new PaginatedItems<AssetInfo>(nextContinuation, entities.Select(p => p.ToDomain(blockchainType)).ToList());
            }
        }

        public async Task<IReadOnlyCollection<AssetInfo>> GetCollectionAsync(string blockchainType, int limit, bool orderAsc, Asset startingAfter = null, Asset endingBefore = null)
        {
            using (var db = new DataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var query = db.AssetInfos
                    .Where(AssetInfosPredicates.BuildEnumeration(startingAfter, endingBefore, orderAsc));

                if (orderAsc)
                {
                    query = query.OrderBy(p => p.Id);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Id);
                }
                
                return (await query.Take(limit).ToListAsync())
                    .Select(p => p.ToDomain(blockchainType)).ToList();
            }
        }
    }
}
