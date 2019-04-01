using System;
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
                _coins.AddOrUpdate(
                    (coin.BlockchainType, coin.Id),
                    key => coin,
                    (key, oldValue) =>
                    {
                        if (oldValue.Version != coin.Version)
                        {
                            throw new InvalidOperationException($"Optimistic concurrency: coin versions mismatch. Expected version {oldValue.Version}, actual {coin.Version}. Coin {coin}.");
                        }

                        var newCoin = new Coin
                        (
                            coin.BlockchainType,
                            coin.Id,
                            coin.Version + 1,
                            coin.Asset,
                            coin.Value,
                            coin.Address,
                            coin.AddressTag,
                            coin.AddressTagType,
                            coin.AddressNonce,
                            coin.SpentByTransactionId
                        );

                        return newCoin;
                    });
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
    }
}
