using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.Contract.Common;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.AzureRepositories
{
    public class InMemoryCoinsRepository : ICoinsRepository
    {
        private readonly ConcurrentDictionary<(string BlockchainType, CoinReference Reference), (ReceivedCoin Coin, string SpentByTransactionId)> _coins;

        public InMemoryCoinsRepository()
        {
            _coins = new ConcurrentDictionary<(string BlockchainType, CoinReference Reference), (ReceivedCoin Coin, string SpentByTransactionId)>();
        }

        public Task SaveAsync(string blockchainType, string transactionId, IEnumerable<ReceivedCoin> coins)
        {
            foreach (var coin in coins)
            {
                _coins.AddOrUpdate(
                    (blockchainType, new CoinReference(transactionId, coin.CoinNumber)),
                    key => (coin, null),
                    (key, oldValue) =>
                    {
                        if (oldValue.SpentByTransactionId != null)
                        {
                            throw new InvalidOperationException($"Optimistic concurrency: coin already was spent by {oldValue.SpentByTransactionId}");
                        }

                        return (coin, null);
                    });
            }

            return Task.CompletedTask;
        }

        public Task<ReceivedCoin> GetToSpendAsync(string blockchainType, CoinReference reference, string toSpendByTransactionId)
        {
            if (!_coins.TryGetValue((blockchainType, reference), out var entity))
            {
                throw new InvalidOperationException($"Coin {reference} not found");
            }

            if (entity.SpentByTransactionId != null && entity.SpentByTransactionId != toSpendByTransactionId)
            {
                throw new InvalidOperationException($"Optimistic concurrency: coin already was spent by {entity.SpentByTransactionId} and can't be spend again by {toSpendByTransactionId}");
            }

            return Task.FromResult(entity.Coin);
        }

        public Task SpendAsync(string blockchainType, CoinReference reference, string byTransactionId)
        {
            throw new NotImplementedException();

            //if (!_coins.TryGetValue((blockchainType, reference), out var entity))
            //{
            //    throw new InvalidOperationException($"Coin {reference} not found");
            //}

            //if (entity.SpentByTransactionId != null && entity.SpentByTransactionId != byTransactionId)
            //{
            //    throw new InvalidOperationException($"Optimistic concurrency: coin already was spent by {entity.SpentByTransactionId} and can't be spend again by {byTransactionId}");
            //}

            //entity.SpentByTransactionId = 

            //return Task.CompletedTask;
        }
    }
}
