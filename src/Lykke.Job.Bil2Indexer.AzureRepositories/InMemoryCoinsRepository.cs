using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.Common;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.AzureRepositories
{
    public class InMemoryCoinsRepository : ICoinsRepository
    {
        private readonly ConcurrentDictionary<(string, CoinReference), Coin> _coins;

        public InMemoryCoinsRepository()
        {
            _coins = new ConcurrentDictionary<(string, CoinReference), Coin>();
        }

        public Task SaveAsync(IEnumerable<Coin> coins)
        {
            foreach (var coin in coins)
            {
                _coins.TryAdd((coin.BlockchainType, coin.Id), coin);
            }

            return Task.CompletedTask;
        }

        public Task SpendAsync(string blockchainType, IEnumerable<CoinReference> ids)
        {
            foreach (var id in ids)
            {
                var key = (blockchainType, id);

                if (!_coins.TryGetValue(key, out var oldValue))
                {
                    continue;
                }

                _coins.TryUpdate
                (
                    key,
                    new Coin
                    (
                        oldValue.BlockchainType,
                        oldValue.Id,
                        oldValue.Version + 1,
                        oldValue.Asset,
                        oldValue.Value,
                        oldValue.Address,
                        oldValue.AddressTag,
                        oldValue.AddressTagType,
                        oldValue.AddressNonce,
                        true
                    ),
                    oldValue
                );
            }

            return Task.CompletedTask;
        }

        public Task RevertSpendingAsync(string blockchainType, IEnumerable<CoinReference> ids)
        {
            foreach (var id in ids)
            {
                var key = (blockchainType, id);

                if (!_coins.TryGetValue(key, out var oldValue))
                {
                    continue;
                }

                _coins.TryUpdate
                (
                    key,
                    new Coin
                    (
                        oldValue.BlockchainType,
                        oldValue.Id,
                        oldValue.Version + 1,
                        oldValue.Asset,
                        oldValue.Value,
                        oldValue.Address,
                        oldValue.AddressTag,
                        oldValue.AddressTagType,
                        oldValue.AddressNonce,
                        false
                    ),
                    oldValue
                );
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlyCollection<Coin>> GetSomeOfAsync(string blockchainType, IEnumerable<CoinReference> ids)
        {
            var coins = ids
                .Select(id =>
                {
                    _coins.TryGetValue((blockchainType, id), out var coin);

                    return coin;
                })
                .Where(x => x != null)
                .ToArray();

            return Task.FromResult<IReadOnlyCollection<Coin>>(coins);
        }

        public Task RemoveIfExistAsync(string blockchainType, IEnumerable<string> receivedInTransactionIds)
        {
            var transactionIds = receivedInTransactionIds.ToHashSet();

            var idsToRemove = _coins.Values.Where(x => transactionIds.Contains(x.Id.TransactionId)).Select(x => x.Id);

            foreach (var id in idsToRemove)
            {
                _coins.TryRemove((blockchainType, id), out _);
            }

            return Task.CompletedTask;
        }
    }
}
