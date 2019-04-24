using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.FeeEnvelopes.Mappers;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Z.EntityFramework.Plus;
using PostgreSQLCopyHelper;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.FeeEnvelopes
{
    public class FeeEnvelopesRepository: IFeeEnvelopesRepository
    {
        private readonly string _posgresConnstring;
        private readonly PostgreSQLCopyHelper<FeeEnvelopeEntity> _copyMapper;

        public FeeEnvelopesRepository(string posgresConnstring)
        {
            _posgresConnstring = posgresConnstring;
            
            _copyMapper = FeeCopyMapper.BuildCopyMapper();
        }

        public async Task AddIfNotExistsAsync(IReadOnlyCollection<FeeEnvelope> fees)
        {
            var dbEntities = fees.Select(p => p.ToDbEntity()).ToList();

            using (var conn = new NpgsqlConnection(_posgresConnstring))
            {
                conn.Open();

                try
                {
                    _copyMapper.SaveAll(conn, dbEntities);
                }
                catch (PostgresException e) when (e.IsNaturalKeyViolationException())
                {
                    var notExisted = await ExcludeExistedInDbAsync(dbEntities);

                    if (notExisted.Any())
                    {
                        _copyMapper.SaveAll(conn, notExisted);
                    }
                }
            }
        }


        private async Task<IReadOnlyCollection<FeeEnvelopeEntity>> ExcludeExistedInDbAsync(IReadOnlyCollection<FeeEnvelopeEntity> dbEntities)
        {
            if (dbEntities.GroupBy(p => p.BlockchainType).Count() > 1)
            {
                throw new ArgumentException("Unable to save batch with multiple blockchain type");
            }

            string BuildId(string bType, string transactionId, string assetId, string assetAddress)
            {
                return $"{bType}_{transactionId}_{assetId}_{assetAddress}";
            }

            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var blockchainType = dbEntities.First().BlockchainType;
                var txIds = dbEntities.Select(p => p.TransactionId.ToString()).ToList();

                var query = db.FeeEnvelopes
                        .Where(p => p.BlockchainType == blockchainType)
                        .Where(p => txIds.Contains(p.TransactionId))
                    .Select(p => new { p.BlockchainType, p.TransactionId, p.AssetId, p.AssetAddress });

                var existedNaturalIds = (await query
                        .ToListAsync())
                    .ToDictionary(p => BuildId(p.BlockchainType, p.TransactionId, p.AssetId, p.AssetAddress));

                var dbEntitiesDic = dbEntities.ToDictionary(p =>
                    BuildId(p.BlockchainType, p.TransactionId, p.AssetId, p.AssetAddress));

                return dbEntitiesDic.Where(p => !existedNaturalIds.ContainsKey(p.Key)).Select(p => p.Value).ToList();
            }
        }

        public async Task<FeeEnvelope> GetOrDefaultAsync(string blockchainType, TransactionId transactionId, Asset asset)
        {
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var entity = await db.FeeEnvelopes
                    .Where(BuildPredicate(blockchainType, transactionId, asset))
                    .SingleOrDefaultAsync();

                return entity?.ToDomain();
            }
        }

        public async Task<FeeEnvelope> GetAsync(string blockchainType, TransactionId transactionId, Asset asset)
        {
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var entity = await db.FeeEnvelopes
                    .Where(BuildPredicate(blockchainType, transactionId, asset))
                    .SingleOrDefaultAsync();

                if (entity == null)
                {
                    throw new ArgumentException($"Fee for {blockchainType}:{transactionId}:{asset} not found");
                }

                return entity.ToDomain();
            }
        }


        public Task<IReadOnlyCollection<FeeEnvelope>> GetTransactionFeesAsync(string blockchainType, TransactionId transactionId)
        {
            return GetAllAsync(BuildPredicate(blockchainType, transactionId));
        }

        public Task<PaginatedItems<FeeEnvelope>> GetBlockFeesAsync(string blockchainType, BlockId blockId, long limit, string continuation)
        {
            return GetPagedAsync(BuildPredicate(blockchainType, blockId), 
                    limit,
                    continuation);
        }

        public async Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId)
        {
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                await db.FeeEnvelopes
                    .Where(BuildPredicate(blockchainType, blockId))
                    .DeleteAsync();
            }
        }

        private async Task<PaginatedItems<FeeEnvelope>> GetPagedAsync(Expression<Func<FeeEnvelopeEntity, bool>> predicate,
            long limit, string continuation)
        {
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                int skip = 0;
                if (!string.IsNullOrEmpty(continuation))
                {
                    skip = int.Parse(continuation);
                }

                var entities = await db.FeeEnvelopes.Where(predicate)
                    .Skip(skip)
                    .Take((int) limit)
                    .ToListAsync();
                
                var nextContinuation = entities.Count < limit ? null : (skip + entities.Count).ToString();

                return new PaginatedItems<FeeEnvelope>(nextContinuation, entities.Select(p=>p.ToDomain()).ToList());
            }
        }


        private async Task<IReadOnlyCollection<FeeEnvelope>> GetAllAsync(Expression<Func<FeeEnvelopeEntity, bool>> predicate)
        {
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var entities = await db.FeeEnvelopes.Where(predicate)
                    .ToListAsync();

                return entities.Select(p => p.ToDomain()).ToList();
            }
        }

        private Expression<Func<FeeEnvelopeEntity, bool>> BuildPredicate(string blockchainType, BlockId blockId)
        {
            var stringBlockId = blockId.ToString();

            return p => p.BlockchainType == blockchainType && p.BlockId == stringBlockId;
        }

        private Expression<Func<FeeEnvelopeEntity, bool>> BuildPredicate(string blockchainType, TransactionId transactionId)
        {
            var stringTransactionId = transactionId.ToString();

            return p => p.BlockchainType == blockchainType
                        && p.TransactionId == stringTransactionId;

        }

        private Expression<Func<FeeEnvelopeEntity, bool>> BuildPredicate(string blockchainType, TransactionId transactionId, Asset asset)
        {
            var stringTransactionId = transactionId.ToString();

            var stringAssetId = asset.Id.ToString();
            var stringAssetAddress = asset.Address?.ToString();

            //force to use filtered index
            if (stringAssetAddress != null)
            {
                return p => p.AssetAddress != null 
                            && p.BlockchainType == blockchainType
                            && p.TransactionId == stringTransactionId 
                            && p.AssetId == stringAssetId 
                            && p.AssetAddress == stringAssetAddress;
            }
            else
            {
                return p => p.AssetAddress == null
                            && p.BlockchainType == blockchainType
                            && p.TransactionId == stringTransactionId
                            && p.AssetId == stringAssetId;
            }
        }

    }
}
