using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.SharedDomain;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;
using Lykke.Numerics;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.FeeEnvelopes
{
    public class FeeEnvelopesRepository: IFeeEnvelopesRepository
    {
        private readonly string _posgresConnString;
        private readonly ILog _log;
        
        public FeeEnvelopesRepository(string posgresConnString, ILogFactory logFactory)
        {
            _posgresConnString = posgresConnString;

            _log = logFactory.CreateLog(this);
        }

        public async Task AddIfNotExistsAsync(IReadOnlyCollection<FeeEnvelope> fees)
        {
            using (var db = new BlockchainDataContext(_posgresConnString))
            {
                var dbEntities = fees.Select(Map).ToList();

                //TODO use COPY instead of insert
                await db.FeeEnvelopes.AddRangeAsync(dbEntities);

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException e)
                {
                    var ids = fees.Select(p => new {p.BlockchainType ,  p.TransactionId, p.Fee.Asset }).ToList();
                    _log.Warning($"Entities already exists, {string.Join(", ", ids)}", exception: e);

                    string BuildId(string blockchainType, string transactionId, string assetId)
                    {
                        return $"{blockchainType}_{transactionId}_{assetId}";
                    }

                    var savedIdData = dbEntities
                        .Select(p => new {p.BlockchainType, p.TransactionId, p.AssetId})
                        .ToList();

                    var existedIds = (await db.FeeEnvelopes
                            .Where(dbEntity => savedIdData.Any(
                                sd => sd.BlockchainType == dbEntity.BlockchainType 
                                      && sd.TransactionId == dbEntity.TransactionId
                                      && sd.AssetId == dbEntity.AssetId ))
                            .Select(p => new {p.BlockchainType, p.TransactionId, p.AssetId})
                            .ToListAsync())
                        .ToDictionary(p => BuildId(p.BlockchainType, p.TransactionId, p.AssetId));

                    var dbEntitiesDic = dbEntities.ToDictionary(p => 
                        BuildId(p.BlockchainType, p.TransactionId, p.AssetId));

                    foreach (var dbEntity in dbEntitiesDic
                        .Where(p => existedIds.ContainsKey(p.Key)))
                    {
                        db.Entry(dbEntity.Value).State = EntityState.Detached;
                    }

                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task<FeeEnvelope> GetOrDefaultAsync(string blockchainType, TransactionId transactionId, Asset asset)
        {
            using (var db = new BlockchainDataContext(_posgresConnString))
            {
                var entity = await db.FeeEnvelopes
                    .SingleOrDefaultAsync(p => p.BlockchainType == blockchainType
                                               && p.TransactionId == transactionId
                                               && p.AssetId == asset.Id);

                return entity != null ? Map(entity) : null;
            }
        }

        public async Task<FeeEnvelope> GetAsync(string blockchainType, TransactionId transactionId, Asset asset)
        {
            using (var db = new BlockchainDataContext(_posgresConnString))
            {
                var entity = await db.FeeEnvelopes
                    .SingleOrDefaultAsync(p => p.BlockchainType == blockchainType
                                               && p.TransactionId == transactionId
                                               && p.AssetId == asset.Id);

                if (entity == null)
                {
                    throw new ArgumentException($"Fee for {blockchainType}:{transactionId}:{asset} not found");
                }

                return Map(entity);
            }
        }


        public Task<IReadOnlyCollection<FeeEnvelope>> GetTransactionFeesAsync(string blockchainType, TransactionId transactionId)
        {
            return GetAllAsync(fee => fee.BlockchainType == blockchainType 
                                   && fee.TransactionId == transactionId);
        }

        public Task<PaginatedItems<FeeEnvelope>> GetBlockFeesAsync(string blockchainType, BlockId blockId, long limit, string continuation)
        {
            return GetPagedAsync(fee => fee.BlockchainType == blockchainType
                                   && fee.BlockId == blockId, 
                    limit,
                    continuation);
        }

        public async Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId)
        {
            using (var db = new BlockchainDataContext(_posgresConnString))
            {
                await db.FeeEnvelopes
                    .Where(p => p.BlockchainType == blockchainType && p.BlockId == blockId)
                    .DeleteAsync();
            }
        }

        private async Task<PaginatedItems<FeeEnvelope>> GetPagedAsync(Expression<Func<FeeEnvelopeEntity, bool>> predicate,
            long limit, string continuation)
        {
            using (var db = new BlockchainDataContext(_posgresConnString))
            {
                int skip = 0;
                if (!string.IsNullOrEmpty(continuation))
                {
                    skip = int.Parse(continuation);
                }

                var entities =await db.FeeEnvelopes.Where(predicate)
                    .Skip(skip)
                    .Take((int) limit)
                    .ToListAsync();


                var nextContinuation = entities.Count < limit ? null : (skip + entities.Count).ToString();

                return new PaginatedItems<FeeEnvelope>(nextContinuation, entities.Select(Map).ToList());
            }
        }

        private async Task<IReadOnlyCollection<FeeEnvelope>> GetAllAsync(Expression<Func<FeeEnvelopeEntity, bool>> predicate)
        {
            using (var db = new BlockchainDataContext(_posgresConnString))
            {
                var entities = await db.FeeEnvelopes.Where(predicate)
                    .ToListAsync();

                return entities.Select(p => Map(p)).ToList();
            }
        }

        private static FeeEnvelope Map(FeeEnvelopeEntity source)
        {
            return new FeeEnvelope(source.BlockchainType, 
                source.BlockId, 
                source.TransactionId, 
                new Fee(new Asset(source.AssetId, source.AssetAddress), new UMoney(new BigInteger(source.Value), source.ValueScale)));
        }


        private static FeeEnvelopeEntity Map(FeeEnvelope source)
        {
            return new FeeEnvelopeEntity
            {
                BlockchainType = source.BlockchainType,
                TransactionId = source.TransactionId,
                AssetAddress = source.Fee.Asset.Address,
                AssetId = source.Fee.Asset.Id,
                BlockId = source.BlockId,
                ValueScale = source.Fee.Amount.Scale
            };
        }

    }
}
