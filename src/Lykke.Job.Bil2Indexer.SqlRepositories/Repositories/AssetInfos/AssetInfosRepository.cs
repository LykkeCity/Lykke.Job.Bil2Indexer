using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.AssetInfos
{
    public class AssetInfosRepository: IAssetInfosRepository
    {
        private readonly string _posgresConnString;

        public AssetInfosRepository(string posgresConnString)
        {
            _posgresConnString = posgresConnString;
        }

        public async Task AddIfNotExistsAsync(IEnumerable<AssetInfo> assets)
        {
            using (var db = new BlockchainDataContext(_posgresConnString))
            {
                foreach (var asset in assets)
                {
                    var dbEntity = Map(asset);
                    await db.AssetInfos.AddAsync(Map(asset));

                    try
                    {
                        await db.SaveChangesAsync();
                    }
                    catch (DbUpdateException e) when(e.IsUniqueConstraintViolationException()) 
                    {
                        var exist = await db.AssetInfos.AnyAsync(p =>
                            p.BlockchainType == asset.BlockchainType && p.Id == asset.Asset.Id);

                        if (exist)
                        {
                            db.Entry(dbEntity).State = EntityState.Detached;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

            }
        }
    

        public async Task<AssetInfo> GetOrDefaultAsync(string blockchainType, Asset asset)
        {
            using (var db = new BlockchainDataContext(_posgresConnString))
            {
                var entity = await db.AssetInfos
                    .SingleOrDefaultAsync(p => p.BlockchainType == blockchainType 
                                               && p.Id == asset.Id);

                return entity != null ? Map(entity) : null;
            }
        }

        public async Task<AssetInfo> GetAsync(string blockchainType, Asset asset)
        {
            using (var db = new BlockchainDataContext(_posgresConnString))
            {
                var entity = await db.AssetInfos
                    .SingleAsync(p => p.BlockchainType == blockchainType
                                               && p.Id == asset.Id);

                return Map(entity);
            }
        }

        public async Task<IReadOnlyCollection<AssetInfo>> GetSomeOfAsync(string blockchainType, IEnumerable<Asset> assets)
        {
            var ids = assets.Select(p => p.Id.ToString());

            using (var db = new BlockchainDataContext(_posgresConnString))
            {
                var entities = await db.AssetInfos
                    .Where(p => p.BlockchainType == blockchainType && ids.Any(x => x == p.Id))
                    .ToListAsync();

                return entities.Select(Map).ToList();
            }
        }

        public async Task<IReadOnlyCollection<AssetInfo>> GetSomeOfAsync(string blockchainType, IEnumerable<AssetId> ids)
        {
            var stringIds = ids.Select(p => p.ToString()).ToList();
            using (var db = new BlockchainDataContext(_posgresConnString))
            {
                var entities = await db.AssetInfos
                    .Where(p => p.BlockchainType == blockchainType
                                      && stringIds.Any(x => x == p.Id))
                    .ToListAsync();

                return entities.Select(Map).ToList();
            }
        }

        public async Task<PaginatedItems<AssetInfo>> GetAllAsync(string blockchainType, int limit, string continuation)
        {
            using (var db = new BlockchainDataContext(_posgresConnString))
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

                return new PaginatedItems<AssetInfo>(nextContinuation, entities.Select(Map).ToList());
            }
        }

        private AssetInfoEntity Map(AssetInfo source)
        {
            return new AssetInfoEntity
            {
                BlockchainType = source.BlockchainType,
                Id = source.Asset.Id,
                Scale = source.Scale
            };
        }

        private AssetInfo Map(AssetInfoEntity source)
        {
            return new AssetInfo(source.BlockchainType, new Asset(source.Id), source.Scale);
        }
    }
}
