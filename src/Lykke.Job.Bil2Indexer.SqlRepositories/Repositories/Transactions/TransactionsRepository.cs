using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions.Mappers;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Z.EntityFramework.Plus;
using PostgreSQLCopyHelper;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions
{
    public class TransactionsRepository : ITransactionsRepository
    {
        private readonly string _postgresConnString;
        private readonly PostgreSQLCopyHelper<TransactionEntity> _copyMapper;

        public TransactionsRepository(string postgresConnString)
        {
            _postgresConnString = postgresConnString;

            _copyMapper = TransactionCopyMapper.BuildCopyMapper();
        }

        public async Task AddIfNotExistsAsync(IEnumerable<Transaction> transactions)
        {
            var entities = transactions
                .Select(t => t.MapToDbEntity())
                .ToArray();

            if (!entities.Any())
            {
                return;
            }

            using (var conn = new NpgsqlConnection(_postgresConnString))
            {
                conn.Open();

                try
                {
                    _copyMapper.SaveAll(conn, entities);
                }
                catch (PostgresException e) when (e.IsNaturalKeyViolationException())
                {
                    var notExisted = await ExcludeExistedInDbAsync(entities);

                    if (notExisted.Any())
                    {
                        _copyMapper.SaveAll(conn, notExisted);
                    }
                }
            }
        }

        public async Task<int> CountInBlockAsync(string blockchainType, BlockId blockId)
        {
            using (var db = new TransactionsDataContext(_postgresConnString))
            {
                return await db.Transactions
                    .Where(BuildPredicate(blockchainType, blockId))
                    .CountAsync();
            }
        }

        public async Task<PaginatedItems<Transaction>> GetAllOfBlockAsync(string blockchainType, BlockId blockId, int limit, string continuation)
        {
            var skip = 0;

            if (!string.IsNullOrEmpty(continuation))
            {
                skip = int.Parse(continuation);
            }

            using (var db = new TransactionsDataContext(_postgresConnString))
            {
                var entities = await db.Transactions
                    .Where(BuildPredicate(blockchainType, blockId))
                    .Skip(skip)
                    .Take(limit)
                    .ToListAsync();

                var nextContinuation = entities.Count < limit ? null : (skip + entities.Count).ToString();

                var envelopes = entities
                    .Select(x => x.MapToTransactionEnvelope())
                    .ToArray();

                return new PaginatedItems<Transaction>(nextContinuation, envelopes);
            }
        }

        public async Task<Transaction> GetAsync(string blockchainType, TransactionId transactionId)
        {
            var envelope = await GetOrDefaultAsync(blockchainType, transactionId);

            if (envelope == null)
            {
                throw new InvalidOperationException($"Transaction {blockchainType}:{transactionId} is not found");
            }

            return envelope;
        }

        public async Task<Transaction> GetOrDefaultAsync(string blockchainType, TransactionId transactionId)
        {
            using (var db = new TransactionsDataContext(_postgresConnString))
            {
                var entity = await db.Transactions
                    .SingleOrDefaultAsync(BuildPredicate(blockchainType, transactionId));

                return entity?.MapToTransactionEnvelope();
            }
        }

        public async Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId)
        {
            using (var db = new TransactionsDataContext(_postgresConnString))
            {
                await db.Transactions
                    .Where(BuildPredicate(blockchainType, blockId))
                    .DeleteAsync();
            }
        }

        private async Task<IReadOnlyCollection<TransactionEntity>> ExcludeExistedInDbAsync(IReadOnlyCollection<TransactionEntity> dbEntities)
        {
            if (dbEntities.GroupBy(p => p.BlockchainType).Count() > 1)
            {
                throw new ArgumentException("Unable to save batch with multiple blockchain type");
            }

            var blockchainType = dbEntities.First().BlockchainType;
            var ids = dbEntities.Select(t => t.TransactionId);

            using (var db = new TransactionsDataContext(_postgresConnString))
            {
                var existedIds = (await db.Transactions.FilterByIds(blockchainType, ids)
                        .Select(t => t.TransactionId)
                        .ToListAsync())
                    .ToHashSet();

                return dbEntities.Where(entity => !existedIds.Contains(entity.TransactionId)).ToArray();
            }
        }

        private Expression<Func<TransactionEntity, bool>> BuildPredicate(string blockchainType, BlockId blockId)
        {
            var stringBlockId = blockId.ToString();

            return p => p.BlockchainType == blockchainType && p.BlockId == stringBlockId;
        }

        private Expression<Func<TransactionEntity, bool>> BuildPredicate(string blockchainType, TransactionId transactionId)
        {
            var stringTransactionId = transactionId.ToString();

            return p => p.BlockchainType == blockchainType && p.TransactionId == stringTransactionId;
        }
    }
}
