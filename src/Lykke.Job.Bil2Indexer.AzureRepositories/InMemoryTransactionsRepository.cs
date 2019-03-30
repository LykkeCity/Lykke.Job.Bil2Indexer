using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.AzureRepositories
{
    public class InMemoryTransactionsRepository : ITransactionsRepository
    {
        private readonly ConcurrentDictionary<(string, string), ConcurrentDictionary<string, bool>> _storage;

        public InMemoryTransactionsRepository()
        {
            _storage = new ConcurrentDictionary<(string, string), ConcurrentDictionary<string, bool>>();
        }

        public Task SaveAsync(string blockchainType, TransferAmountTransactionExecutedEvent transaction)
        {
            SaveTransaction(blockchainType, transaction.BlockId, transaction.TransactionId);

            return Task.CompletedTask;
        }

        public Task SaveAsync(string blockchainType, TransferCoinsTransactionExecutedEvent transaction)
        {
            SaveTransaction(blockchainType, transaction.BlockId, transaction.TransactionId);

            return Task.CompletedTask;
        }

        public Task SaveAsync(string blockchainType, TransactionFailedEvent transaction)
        {
            SaveTransaction(blockchainType, transaction.BlockId, transaction.TransactionId);

            return Task.CompletedTask;
        }

        public Task<int> CountInBlockAsync(string blockchainType, string blockId)
        {
            if (!_storage.TryGetValue((blockchainType, blockId), out var set))
            {
                return Task.FromResult(0);
            }

            return Task.FromResult(set.Count);
        }

        private void SaveTransaction(string blockchainType, string blockId, string transactionId)
        {
            Console.WriteLine($"Transaction saved: {blockchainType}:{blockId}:{transactionId}");

            _storage.AddOrUpdate(
                (blockchainType, blockId),
                key =>
                {
                    var set = new ConcurrentDictionary<string, bool>();

                    set.TryAdd(transactionId, false);

                    return set;
                },
                (key, set) =>
                {
                    set.TryAdd(transactionId, false);

                    return set;
                });
        }
    }
}
