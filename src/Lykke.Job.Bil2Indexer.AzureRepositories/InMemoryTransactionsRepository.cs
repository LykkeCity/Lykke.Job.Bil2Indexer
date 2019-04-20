using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.SharedDomain;
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

        public Task AddIfNotExistsAsync(string blockchainType, TransferAmountTransactionExecutedEvent transaction)
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

        public Task AddIfNotExistsAsync(string blockchainType, TransferCoinsTransactionExecutedEvent transaction)
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

        public Task AddIfNotExistsAsync(string blockchainType, TransactionFailedEvent transaction)
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

        public Task<int> CountInBlockAsync(string blockchainType, BlockId blockId)
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
            BlockId blockId,
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

        public async Task<PaginatedItems<TransferAmountTransactionExecutedEvent>> GetTransferAmountTransactionsOfBlockAsync(
            string blockchainType, BlockId blockId, int limit, string continuation)
        {
            if (_storage.TryGetValue((blockchainType, blockId.Value), out var transactions))
            {
                int skip = 0;
                int.TryParse(continuation, out skip);

                var items = transactions
                    .TransferAmountTransactions
                    .Select(x => x.Value)
                    .Skip(skip)
                    .Take(limit)
                    .ToArray();

                var nextContinuation = items.Length > 0 ? (skip + items.Length).ToString() : "";

                return new PaginatedItems<TransferAmountTransactionExecutedEvent>(nextContinuation, items);
            }

            return new PaginatedItems<TransferAmountTransactionExecutedEvent>(null, new TransferAmountTransactionExecutedEvent[0]);
        }

        public async Task<PaginatedItems<TransactionFailedEvent>> GetFailedTransactionsOfBlockAsync(string blockchainType,
            BlockId blockId, int limit, string continuation)
        {
            return new PaginatedItems<TransactionFailedEvent>(null, new List<TransactionFailedEvent>() { });
        }

        public Task<TransferCoinsTransactionExecutedEvent> GetTransferCoinsTransactionAsync(string blockchainType,
            TransactionId transactionId)
        {
            throw new NotImplementedException();
        }

        public Task<TransferAmountTransactionExecutedEvent> GetTransferAmountTransactionAsync(string blockchainType,
            TransactionId transactionId)
        {
            throw new NotImplementedException();
        }

        public Task<TransactionFailedEvent> GetFailedTransactionAsync(string blockchainType,
            TransactionId transactionId)
        {
            throw new NotImplementedException();
        }

        public Task<TransferCoinsTransactionExecutedEvent> GetTransferCoinsTransactionOrDefaultAsync(
            string blockchainType, TransactionId transactionId)
        {
            throw new NotImplementedException();
        }

        public Task<TransferAmountTransactionExecutedEvent> GetTransferAmountTransactionOrDefaultAsync(
            string blockchainType, TransactionId transactionId)
        {
            throw new NotImplementedException();
        }

        public Task<TransactionFailedEvent> GetFailedTransactionOrDefaultAsync(string blockchainType,
            TransactionId transactionId)
        {
            throw new NotImplementedException();
        }

        public Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId)
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
