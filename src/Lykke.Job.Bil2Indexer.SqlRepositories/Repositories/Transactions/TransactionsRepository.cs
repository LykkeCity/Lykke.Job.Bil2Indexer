using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.SharedDomain;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions.Mappers;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;
namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions
{
    public class TransactionsRepository : ITransactionsRepository
    {
        private readonly string _postgresConnString;
        private readonly ILog _log;

        public TransactionsRepository(string postgresConnString, ILogFactory logFactory)
        {
            _postgresConnString = postgresConnString;
            _log = logFactory.CreateLog(this);
        }

        public Task AddIfNotExistsAsync(string blockchainType, TransferAmountTransactionExecutedEvent transaction)
        {
            return AddIfNotExistsAsync(transaction.MapToDbEntity(blockchainType));
        }

        public Task AddIfNotExistsAsync(string blockchainType, TransferCoinsTransactionExecutedEvent transaction)
        {
            return AddIfNotExistsAsync(transaction.MapToDbEntity(blockchainType));
        }

        public Task AddIfNotExistsAsync(string blockchainType, TransactionFailedEvent transaction)
        {
            return AddIfNotExistsAsync(transaction.MapToDbEntity(blockchainType));
        }

        private async Task AddIfNotExistsAsync(TransactionEntity transaction)
        {
            using (var db = new TransactionsDataContext(_postgresConnString))
            {
                await db.Transactions.AddAsync(transaction);

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException e) when(e.IsUniqueConstraintViolationException())
                {
                    var exist = await db.Transactions.AnyAsync(p =>
                        p.BlockchainType == transaction.BlockchainType &&
                        p.TransactionId == transaction.TransactionId);

                    if (!exist)
                    {
                        throw;
                    }

                    _log.Info("Transaction already exists. Skipping", 
                        context: transaction,
                        exception: e);
                }
            }
        }

        public async Task<int> CountInBlockAsync(string blockchainType, BlockId blockId)
        {
            using (var db = new TransactionsDataContext(_postgresConnString))
            {
                return await db.Transactions
                    .Where(p => p.BlockchainType == blockchainType && p.BlockId == blockId)
                    .CountAsync();
            }
        }

        public async Task<PaginatedItems<TransactionEnvelope>> GetAllOfBlockAsync(string blockchainType, BlockId blockId, int limit, string continuation)
        {
            var skip = 0;

            if (!string.IsNullOrEmpty(continuation))
            {
                skip = int.Parse(continuation);
            }

            using (var db = new TransactionsDataContext(_postgresConnString))
            {
                var entities = await db.Transactions
                    .Where(p => p.BlockchainType == blockchainType && p.BlockId == blockId)
                    .Skip(skip)
                    .Take(limit)
                    .ToListAsync();

                var nextContinuation = entities.Count < limit ? null : (skip + entities.Count).ToString();

                var envelopes = entities
                    .Select(x => x.MapToTransactionEnvelope())
                    .ToArray();

                return new PaginatedItems<TransactionEnvelope>(nextContinuation, envelopes);
            }
        }

        public async Task<TransactionEnvelope> GetAsync(string blockchainType, TransactionId transactionId)
        {
            var envelope = await GetOrDefaultAsync(blockchainType, transactionId);

            if (envelope == null)
            {
                throw new InvalidOperationException($"Transaction {blockchainType}:{transactionId} is not found");
            }

            return envelope;
        }

        public async Task<TransactionEnvelope> GetOrDefaultAsync(string blockchainType, TransactionId transactionId)
        {
            using (var db = new TransactionsDataContext(_postgresConnString))
            {
                var entity = await db.Transactions
                    .SingleOrDefaultAsync(p => p.BlockchainType == blockchainType && p.TransactionId == transactionId);

                return entity?.MapToTransactionEnvelope();
            }
        }

        public async Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId)
        {
            using (var db = new TransactionsDataContext(_postgresConnString))
            {
                await db.Transactions
                    .Where(p => p.BlockchainType == blockchainType && p.BlockId == blockId)
                    .DeleteAsync();
            }
        }
    }
}
