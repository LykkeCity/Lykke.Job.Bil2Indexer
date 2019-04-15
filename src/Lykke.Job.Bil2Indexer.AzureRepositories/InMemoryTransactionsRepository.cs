using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.AzureRepositories
{
    public class InMemoryTransactionsRepository : ITransactionsRepository
    {
        private class BlockTransactions
        {
            public ConcurrentDictionary<string, TransferAmountTransactionExecutedEvent> TransferAmountTransactions { get; }
            public ConcurrentDictionary<string, TransferCoinsTransactionExecutedEvent> TransferCoinsTransactions { get; }
            public ConcurrentDictionary<string, TransactionFailedEvent> FailedTransactions { get; }

            public BlockTransactions()
            {
                TransferAmountTransactions = new ConcurrentDictionary<string, TransferAmountTransactionExecutedEvent>();
                TransferCoinsTransactions = new ConcurrentDictionary<string, TransferCoinsTransactionExecutedEvent>();
                FailedTransactions = new ConcurrentDictionary<string, TransactionFailedEvent>();
            }
        }

        private readonly ConcurrentDictionary<(string, string), BlockTransactions> _storage;
        private readonly ILog _log;

        public InMemoryTransactionsRepository(ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);

            _storage = new ConcurrentDictionary<(string, string), BlockTransactions>();
        }

        public Task SaveAsync(string blockchainType, TransferAmountTransactionExecutedEvent transaction)
        {
            SaveTransaction
            (
                blockchainType,
                transaction.BlockId,
                transaction.TransactionId,
                (key, blockTransactions) => blockTransactions.TransferAmountTransactions.TryAdd(key, transaction)
            );

            return Task.CompletedTask;
        }

        public Task SaveAsync(string blockchainType, TransferCoinsTransactionExecutedEvent transaction)
        {
            SaveTransaction
            (
                blockchainType,
                transaction.BlockId,
                transaction.TransactionId,
                (key, blockTransactions) => blockTransactions.TransferCoinsTransactions.TryAdd(key, transaction)
            );

            return Task.CompletedTask;
        }

        public Task SaveAsync(string blockchainType, TransactionFailedEvent transaction)
        {
            SaveTransaction
            (
                blockchainType,
                transaction.BlockId,
                transaction.TransactionId,
                (key, blockTransactions) => blockTransactions.FailedTransactions.TryAdd(key, transaction)
            );

            return Task.CompletedTask;
        }

        public Task<int> CountInBlockAsync(string blockchainType, string blockId)
        {
            if (!_storage.TryGetValue((blockchainType, blockId), out var blockTransactions))
            {
                return Task.FromResult(0);
            }

            var totalCount = blockTransactions.FailedTransactions.Count +
                             blockTransactions.TransferAmountTransactions.Count +
                             blockTransactions.TransferCoinsTransactions.Count;

            return Task.FromResult(totalCount);
        }

        public Task<PaginatedItems<TransferCoinsTransactionExecutedEvent>> GetTransferCoinsTransactionsOfBlockAsync(
            string blockchainType,
            string blockId,
            int limit,
            string continuation)
        {
            if (_storage.TryGetValue((blockchainType, blockId), out var blockTransactions))
            {
                var transactions = blockTransactions.TransferCoinsTransactions.Values.ToArray();
                var paginatedItems = new PaginatedItems<TransferCoinsTransactionExecutedEvent>
                (
                    null,
                    transactions
                );

                return Task.FromResult(paginatedItems);
            }

            return Task.FromResult(PaginatedItems<TransferCoinsTransactionExecutedEvent>.Empty);
        }

        public Task<PaginatedItems<TransferAmountTransactionExecutedEvent>> GetTransferAmountTransactionsOfBlockAsync(
            string blockchainType, string blockId, int limit, string continuation)
        {
            throw new NotImplementedException();
        }

        public Task<PaginatedItems<TransactionFailedEvent>> GetFailedTransactionsOfBlockAsync(string blockchainType,
            string blockId, int limit, string continuation)
        {
            throw new NotImplementedException();
        }

        public Task<TransferCoinsTransactionExecutedEvent> GetTransferCoinsTransactionAsync(string blockchainType, string transactionId)
        {
            throw new NotImplementedException();
        }

        public Task<TransferAmountTransactionExecutedEvent> GetTransferAmountTransactionAsync(string blockchainType, string transactionId)
        {
            throw new NotImplementedException();
        }

        public Task<TransactionFailedEvent> GetFailedTransactionAsync(string blockchainType, string transactionId)
        {
            throw new NotImplementedException();
        }

        public Task<TransferCoinsTransactionExecutedEvent> GetTransferCoinsTransactionOrDefaultAsync(string blockchainType, string transactionId)
        {
            throw new NotImplementedException();
        }

        public Task<TransferAmountTransactionExecutedEvent> GetTransferAmountTransactionOrDefaultAsync(string blockchainType, string transactionId)
        {
            throw new NotImplementedException();
        }

        public Task<TransactionFailedEvent> GetFailedTransactionOrDefaultAsync(string blockchainType, string transactionId)
        {
            throw new NotImplementedException();
        }

        public Task TryRemoveAllOfBlockAsync(string blockchainType, string blockId)
        {
            if (_storage.TryRemove((blockchainType, blockId), out _))
            {
                _log.Info($"Transaction of block: {blockchainType}:{blockId} removed");
            }

            return Task.CompletedTask;
        }

        private void SaveTransaction(string blockchainType, string blockId, string transactionId, Action<string, BlockTransactions> addTransaction)
        {
            _log.Info($"Transaction saved: {blockchainType}:{blockId}:{transactionId}");

            _storage.AddOrUpdate(
                (blockchainType, blockId),
                key =>
                {
                    var blockTransactions = new BlockTransactions();

                    addTransaction.Invoke(transactionId, blockTransactions);

                    return blockTransactions;
                },
                (key, blockTransactions) =>
                {
                    addTransaction.Invoke(transactionId, blockTransactions);

                    return blockTransactions;
                });
        }
    }
}
