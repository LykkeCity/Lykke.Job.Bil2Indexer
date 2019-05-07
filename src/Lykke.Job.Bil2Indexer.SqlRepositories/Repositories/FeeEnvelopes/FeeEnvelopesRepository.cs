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
        private readonly IPgConnectionStringProvider _connectionStringProvider;
        private readonly PostgreSQLCopyHelper<FeeEnvelopeEntity> _copyMapper;

        public FeeEnvelopesRepository(IPgConnectionStringProvider connectionStringProvider)
        {
            _connectionStringProvider = connectionStringProvider;
            _copyMapper = FeeCopyMapper.BuildCopyMapper();
        }

        public async Task AddIfNotExistsAsync(IReadOnlyCollection<FeeEnvelope> fees)
        {
            var dbEntities = fees.Select(p => p.ToDbEntity()).ToList();

            if (!dbEntities.Any())
            {
                return;
            }

            using (var conn = new NpgsqlConnection(_connectionStringProvider.GetConnectionString(fees.First().BlockchainType)))
            {
                conn.Open();

                try
                {
                    _copyMapper.SaveAll(conn, dbEntities);
                }
                catch (PostgresException e) when (e.IsNaturalKeyViolationException())
                {
                    var notExisted = await ExcludeExistedInDbAsync(fees.First().BlockchainType, dbEntities);

                    if (notExisted.Any())
                    {
                        _copyMapper.SaveAll(conn, notExisted);
                    }
                }
            }
        }
        
        private async Task<IReadOnlyCollection<FeeEnvelopeEntity>> ExcludeExistedInDbAsync(string blockchainType, IReadOnlyCollection<FeeEnvelopeEntity> dbEntities)
        {
            string BuildId(string transactionId, string assetId, string assetAddress)
            {
                return $"{transactionId}_{assetId}_{assetAddress}";
            }

            using (var db = new BlockchainDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var txIdsWithAssetAddress = dbEntities
                    .Where(p => p.AssetAddress != null)
                    .Select(p => p.TransactionId)
                    .ToList();

                var txIdsWithoutAssetAddress = dbEntities
                    .Where(p => p.AssetAddress == null)
                    .Select(p => p.TransactionId)
                    .ToList();

                var getNaturalIds1 = db.FeeEnvelopes
                    .Where(FeeEnvelopePredicates.Build(txIdsWithAssetAddress, isAssetAddressNull: false))
                    .Select(p => new { p.TransactionId, p.AssetId, p.AssetAddress })
                    .ToListAsync();

                var getNaturalIds2 = db.FeeEnvelopes
                    .Where(FeeEnvelopePredicates.Build(txIdsWithoutAssetAddress, isAssetAddressNull: true))
                    .Select(p => new { p.TransactionId, p.AssetId, p.AssetAddress })
                    .ToListAsync();

                await Task.WhenAll(getNaturalIds1, getNaturalIds2);

                var existedNaturalIds = getNaturalIds1.Result.Union(getNaturalIds2.Result)
                    .ToDictionary(p => BuildId(p.TransactionId, p.AssetId, p.AssetAddress));

                var dbEntitiesDic = dbEntities.ToDictionary(p =>
                    BuildId(p.TransactionId, p.AssetId, p.AssetAddress));

                return dbEntitiesDic.Where(p => !existedNaturalIds.ContainsKey(p.Key)).Select(p => p.Value).ToList();
            }
        }

        public async Task<FeeEnvelope> GetOrDefaultAsync(string blockchainType, TransactionId transactionId, Asset asset)
        {
            using (var db = new BlockchainDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var entity = await db.FeeEnvelopes
                    .Where(FeeEnvelopePredicates.Build(transactionId, asset))
                    .SingleOrDefaultAsync();

                return entity?.ToDomain(blockchainType);
            }
        }

        public async Task<FeeEnvelope> GetAsync(string blockchainType, TransactionId transactionId, Asset asset)
        {
            using (var db = new BlockchainDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var entity = await db.FeeEnvelopes
                    .Where(FeeEnvelopePredicates.Build(transactionId, asset))
                    .SingleOrDefaultAsync();

                if (entity == null)
                {
                    throw new ArgumentException($"Fee for {blockchainType}:{transactionId}:{asset} not found");
                }

                return entity.ToDomain(blockchainType);
            }
        }


        public Task<IReadOnlyCollection<FeeEnvelope>> GetTransactionFeesAsync(string blockchainType, TransactionId transactionId)
        {
            return GetAllAsync(blockchainType, FeeEnvelopePredicates.Build(transactionId));
        }

        public Task<PaginatedItems<FeeEnvelope>> GetBlockFeesAsync(string blockchainType, BlockId blockId, long limit, string continuation)
        {
            return GetPagedAsync(blockchainType, 
                    FeeEnvelopePredicates.Build(blockId), 
                    limit,
                    continuation);
        }

        public async Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId)
        {
            using (var db = new BlockchainDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                await db.FeeEnvelopes
                    .Where(FeeEnvelopePredicates.Build(blockId))
                    .DeleteAsync();
            }
        }

        private async Task<PaginatedItems<FeeEnvelope>> GetPagedAsync(string blockchainType, Expression<Func<FeeEnvelopeEntity, bool>> predicate,
            long limit, string continuation)
        {
            using (var db = new BlockchainDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
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

                return new PaginatedItems<FeeEnvelope>(nextContinuation, entities.Select(p => p.ToDomain(blockchainType)).ToList());
            }
        }
        
        private async Task<IReadOnlyCollection<FeeEnvelope>> GetAllAsync(string blockchainType, Expression<Func<FeeEnvelopeEntity, bool>> predicate)
        {
            using (var db = new BlockchainDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var entities = await db.FeeEnvelopes.Where(predicate)
                    .ToListAsync();

                return entities.Select(p => p.ToDomain(blockchainType)).ToList();
            }
        }
    }
}
