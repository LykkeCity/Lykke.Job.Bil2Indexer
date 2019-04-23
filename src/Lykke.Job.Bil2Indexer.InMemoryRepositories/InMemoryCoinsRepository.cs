using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.InMemoryRepositories
{
    public class InMemoryCoinsRepository : ICoinsRepository
    {
        private readonly ConcurrentDictionary<(string, CoinId), Coin> _coins;

        public InMemoryCoinsRepository()
        {
            _coins = new ConcurrentDictionary<(string, CoinId), Coin>();
        }

        public Task AddIfNotExistsAsync(IReadOnlyCollection<Coin> coins)
        {
            foreach (var coin in coins)
            {
                _coins.TryAdd((coin.BlockchainType, coin.Id), coin);
            }

            return Task.CompletedTask;
        }

        public Task SpendAsync(string blockchainType, IReadOnlyCollection<CoinId> ids)
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

        public Task RevertSpendingAsync(string blockchainType, IReadOnlyCollection<CoinId> ids)
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

        public Task<IReadOnlyCollection<Coin>> GetSomeOfAsync(string blockchainType, IReadOnlyCollection<CoinId> ids)
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

        public Task RemoveIfExistAsync(string blockchainType, ISet<TransactionId> receivedInTransactionIds)
        {
            var idsToRemove = _coins.Values.Where(x => receivedInTransactionIds.Contains(x.Id.TransactionId)).Select(x => x.Id);

            foreach (var id in idsToRemove)
            {
                _coins.TryRemove((blockchainType, id), out _);
            }

            return Task.CompletedTask;
        }
    }
}
