using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions.Mappers;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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

        public Task SaveAsync(string blockchainType, TransferAmountTransactionExecutedEvent transaction)
        {
            return SaveAsync(transaction.MapToDbEntity(blockchainType));
        }

        public Task SaveAsync(string blockchainType, TransferCoinsTransactionExecutedEvent transaction)
        {
            return SaveAsync(transaction.MapToDbEntity(blockchainType));
        }

        public Task SaveAsync(string blockchainType, TransactionFailedEvent transaction)
        {
            return SaveAsync(transaction.MapToDbEntity(blockchainType));
        }

        private async Task SaveAsync(TransactionEntity transaction)
        {
            using (var db = new TransactionsDataContext(_posgresConnString))
            {
                db.Transactions.Add(transaction);

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException e) when (IsConstraintViolationException(e))
                {
                    _log.Info("Transaction already exists. Skipping", context: transaction, exception: e);
                }
            }
        }

        private bool IsConstraintViolationException(DbUpdateException e)
        {
            const string constraintViolationErrorCode = "23505";
            const string uniqueConstraintName = "transactions_blockchain_type_transaction_id_transaction_id_uind";

            if (e.InnerException is PostgresException pgEx)
            {
                if(string.Equals(pgEx.SqlState, constraintViolationErrorCode, StringComparison.InvariantCultureIgnoreCase)
                   && string.Equals(pgEx.ConstraintName, uniqueConstraintName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public Task<int> CountInBlockAsync(string blockchainType, string blockId)
        {
            using (var db = new TransactionsDataContext(_posgresConnString))
            {
                return db.Transactions
                    .Where(p => p.BlockchainType == blockchainType && p.BlockId == blockId)
                    .CountAsync();
            }
        }

        public async Task<PaginatedItems<TransferCoinsTransactionExecutedEvent>> GetTransferCoinsTransactionsOfBlockAsync(string blockchainType, string blockId, string continuation)
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

                return new PaginatedItems<TransferCoinsTransactionExecutedEvent>(nextContinuation, entities.Select(p => p.MapToCoinExecuted()).ToList());
            }
        }

        public Task TryRemoveAllOfBlockAsync(string blockchainType, string blockId)
        {
            using (var db = new TransactionsDataContext(_posgresConnString))
            {
                return db.Transactions.Where(p => p.BlockchainType == blockchainType && p.BlockId == blockId)
                    .DeleteAsync();
            }
        }
    }
}
