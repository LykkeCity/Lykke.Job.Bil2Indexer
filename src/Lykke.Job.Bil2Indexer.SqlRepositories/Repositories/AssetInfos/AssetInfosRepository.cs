﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            foreach (var asset in assets)
            {
                using (var db = new BlockchainDataContext(_posgresConnString))
                {
                    var dbEntity = asset.ToDbEntity();

                    await db.AssetInfos.AddAsync(dbEntity);

                    try
                    {
                        await db.SaveChangesAsync();
                    }
                    catch (DbUpdateException e) when (e.IsUniqueConstraintViolationException())
                    {
                        var exist = await db.AssetInfos.AnyAsync(BuildPredicate(asset.BlockchainType, asset.Asset));

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
                    .SingleOrDefaultAsync(BuildPredicate(blockchainType, asset));

                return entity?.ToDomain();
            }
        }

        public async Task<AssetInfo> GetAsync(string blockchainType, Asset asset)
        {
            using (var db = new BlockchainDataContext(_posgresConnString))
            {
                var entity = await db.AssetInfos
                    .SingleAsync(BuildPredicate(blockchainType, asset));

                return entity.ToDomain();
            }
        }

        public async Task<IReadOnlyCollection<AssetInfo>> GetSomeOfAsync(string blockchainType, IEnumerable<Asset> assets)
        {
            var ids = assets.Select(AssetInfoMapper.BuildId).ToList();

            using (var db = new BlockchainDataContext(_posgresConnString))
            {
                var entities = await db.AssetInfos
                    .Where(p => p.BlockchainType == blockchainType && ids.Contains(p.Id))
                    .ToListAsync();

                return entities.Select(p => p.ToDomain()).ToList();
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

                return new PaginatedItems<AssetInfo>(nextContinuation, entities.Select(p => p.ToDomain()).ToList());
            }
        }

        private Expression<Func<AssetInfoEntity, bool>> BuildPredicate(string blockchainType, Asset asset)
        {
            var id = AssetInfoMapper.BuildId(asset);

            return dbEntity => dbEntity.BlockchainType == blockchainType
                               && dbEntity.Id == id;
        }
    }
}
