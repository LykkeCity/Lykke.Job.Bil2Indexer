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
                catch (PostgresException e) when (e.IsUniqueConstraintViolationException())
                {
                    var notExisted = await ExcludeExistedInDbAsync(dbEntities);

                    _copyMapper.SaveAll(conn, notExisted);
                }
            }
        }


        private async Task<IReadOnlyCollection<FeeEnvelopeEntity>> ExcludeExistedInDbAsync(IReadOnlyCollection<FeeEnvelopeEntity> dbEntities)
        {
            if (dbEntities.GroupBy(p => p.AssetId).Count() > 1)
            {
                throw new ArgumentException("Unable to save batch with multiple assetIds");
            }

            if (dbEntities.GroupBy(p => p.BlockchainType).Count() > 1)
            {
                throw new ArgumentException("Unable to save batch with multiple blockchain type");
            }

            string BuildId(string bType, string transactionId, string assetId)
            {
                return $"{bType}_{transactionId}_{assetId}";
            }

            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var blockchainType = dbEntities.First().BlockchainType;
                var txIds = dbEntities.Select(p => p.TransactionId).ToList();
                var assetId = dbEntities.First().AssetId;

                var query = db.FeeEnvelopes
                        .Where(p => p.BlockchainType == blockchainType)
                        .Where(p=> txIds.Contains(p.TransactionId))
                        .Where(p => p.AssetId == assetId)
                    .Select(p => new { p.BlockchainType, p.TransactionId, p.AssetId });

                var existedNaturalIds = (await query
                        .ToListAsync())
                    .ToDictionary(p => BuildId(p.BlockchainType, p.TransactionId, p.AssetId));

                var dbEntitiesDic = dbEntities.ToDictionary(p =>
                    BuildId(p.BlockchainType, p.TransactionId, p.AssetId));

                return dbEntitiesDic.Where(p => !existedNaturalIds.ContainsKey(p.Key)).Select(p => p.Value).ToList();
            }
        }

        public async Task<FeeEnvelope> GetOrDefaultAsync(string blockchainType, TransactionId transactionId, Asset asset)
        {
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var entity = await db.FeeEnvelopes
                    .Where(p => p.BlockchainType == blockchainType
                                               && p.TransactionId == transactionId
                                               && p.AssetId == asset.Id)
                    .SingleOrDefaultAsync();

                return entity?.ToDomain();
            }
        }

        public async Task<FeeEnvelope> GetAsync(string blockchainType, TransactionId transactionId, Asset asset)
        {
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var entity = await db.FeeEnvelopes
                    .Where(p => p.BlockchainType == blockchainType
                                               && p.TransactionId == transactionId
                                               && p.AssetId == asset.Id)
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
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                await db.FeeEnvelopes
                    .Where(p => p.BlockchainType == blockchainType && p.BlockId == blockId)
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

                var entities =await db.FeeEnvelopes.Where(predicate)
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

    }
}
