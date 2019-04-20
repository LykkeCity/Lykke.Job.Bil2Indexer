using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.SharedDomain;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions.Mappers;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;
namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions
{
    public class TransactionsRepository: ITransactionsRepository
    {
        private readonly string _posgresConnString;
        private readonly ILog _log;

        public TransactionsRepository(string posgresConnString, ILogFactory logFactory)
        {
            _posgresConnString = posgresConnString;
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
            using (var db = new TransactionsDataContext(_posgresConnString))
            {
                await db.Transactions.AddAsync(transaction);

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException e) when(e.IsConstraintViolationException())
                {
                    var exist = await db.Transactions.AnyAsync(p =>
                        p.BlockchainType == transaction.BlockchainType &&
                        p.TransactionId == transaction.TransactionId);

                    if (!exist)
                    {
                        throw;
                    }

                    _log.Info("Transaction already exists. Skiping", 
                        context: transaction,
                        exception: e);
                }
            }
        }

        public async Task<int> CountInBlockAsync(string blockchainType, BlockId blockId)
        {
            using (var db = new TransactionsDataContext(_posgresConnString))
            {
                return await db.Transactions
                    .Where(p => p.BlockchainType == blockchainType && p.BlockId == blockId)
                    .CountAsync();
            }
        }

        public async Task<PaginatedItems<TransferCoinsTransactionExecutedEvent>> GetTransferCoinsTransactionsOfBlockAsync(string blockchainType, BlockId blockId, int limit, string continuation)
        {
            return await GetPagedAsync(blockchainType, 
                blockId, 
                continuation,
                p => p.MapToCoinExecuted());
        }

        public async Task<PaginatedItems<TransferAmountTransactionExecutedEvent>> GetTransferAmountTransactionsOfBlockAsync(string blockchainType, BlockId blockId, int limit, string continuation)
        {
            return await GetPagedAsync(blockchainType, 
                blockId, 
                continuation,
                p => p.MapToTransferAmountExecuted());
        }

        public async Task<PaginatedItems<TransactionFailedEvent>> GetFailedTransactionsOfBlockAsync(string blockchainType, BlockId blockId, int limit, string continuation)
        {
            return await GetPagedAsync(blockchainType,
                blockId, 
                continuation, 
                p => p.MapToFailed());
        }

        public async Task<TransferCoinsTransactionExecutedEvent> GetTransferCoinsTransactionAsync(string blockchainType, TransactionId transactionId)
        {
            return await GetOrDefaultAsync(blockchainType, 
                       transactionId, 
                       p => p.MapToCoinExecuted())
                   ?? throw new ArgumentException($"Unable to find transaction {blockchainType} : {transactionId}");
        }

        public async Task<TransferAmountTransactionExecutedEvent> GetTransferAmountTransactionAsync(string blockchainType, TransactionId transactionId)
        {
            return await GetOrDefaultAsync(blockchainType, 
                       transactionId, 
                       p => p.MapToTransferAmountExecuted())
                   ?? throw new ArgumentException($"Unable to find transaction {blockchainType} : {transactionId}");
        }

        public async Task<TransactionFailedEvent> GetFailedTransactionAsync(string blockchainType, TransactionId transactionId)
        {
            return await GetOrDefaultAsync(blockchainType, 
                       transactionId, 
                       p => p.MapToFailed())
                   ?? throw new ArgumentException($"Unable to find transaction {blockchainType} : {transactionId}");
        }

        public async Task<TransferCoinsTransactionExecutedEvent> GetTransferCoinsTransactionOrDefaultAsync(string blockchainType, TransactionId transactionId)
        {
            return await GetOrDefaultAsync(blockchainType, 
                transactionId, 
                p => p.MapToCoinExecuted());
        }

        public async Task<TransferAmountTransactionExecutedEvent> GetTransferAmountTransactionOrDefaultAsync(string blockchainType, TransactionId transactionId)
        {
            return await GetOrDefaultAsync(blockchainType, 
                transactionId, 
                p => p.MapToTransferAmountExecuted());
        }

        public async Task<TransactionFailedEvent> GetFailedTransactionOrDefaultAsync(string blockchainType, TransactionId transactionId)
        {
            return await GetOrDefaultAsync(blockchainType, 
                transactionId, 
                p => p.MapToFailed());
        }

        private async Task<PaginatedItems<T>> GetPagedAsync<T>(
            string blockchainType, string blockId, string continuation, Func<TransactionEntity, T> map)
        {
            const int take = 50;

            var skip = 0;
            if (!string.IsNullOrEmpty(continuation))
            {
                skip = int.Parse(continuation);
            }

            using (var db = new TransactionsDataContext(_posgresConnString))
            {
                var entities = await db.Transactions
                    .Where(p => p.BlockchainType == blockchainType && p.BlockId == blockId)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();

                var nextContinuation = entities.Count < take ? null : (skip + entities.Count).ToString();

                return new PaginatedItems<T>(nextContinuation, entities.Select(map).ToList());
            }
        }

        private async Task<T> GetOrDefaultAsync<T>(string blockchainType, string transactionId, Func<TransactionEntity, T> map) where T:class 
        {
            using (var db = new TransactionsDataContext(_posgresConnString))
            {
                var entity = await db.Transactions.SingleOrDefaultAsync(p =>
                    p.BlockchainType == blockchainType && p.TransactionId == transactionId);

                return entity != null ? map(entity) : null;
            }
        }

        public async Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId)
        {
            using (var db = new TransactionsDataContext(_posgresConnString))
            {
                await db.Transactions
                    .Where(p => p.BlockchainType == blockchainType && p.BlockId == blockId)
                    .DeleteAsync();
            }
        }
    }
}
