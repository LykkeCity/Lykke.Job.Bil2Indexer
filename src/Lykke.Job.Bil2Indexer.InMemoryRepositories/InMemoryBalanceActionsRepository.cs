using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Contract;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.InMemoryRepositories
{
    public class InMemoryBalanceActionsRepository : IBalanceActionsRepository
    {
        private readonly ConcurrentDictionary<(string, AccountId), List<BalanceAction>> _actions;

        public InMemoryBalanceActionsRepository()
        {
            _actions = new ConcurrentDictionary<(string, AccountId), List<BalanceAction>>();
        }

        public Task AddIfNotExistsAsync(string blockchainType, IEnumerable<BalanceAction> actions)
        {
            foreach (var action in actions)
            {
                _actions.AddOrUpdate(
                    (blockchainType, action.AccountId),
                    key => new List<BalanceAction>
                    {
                        action
                    },
                    (key, accountActions) =>
                    {
                        lock (accountActions)
                        {
                            accountActions.Add(action);
                        }

                        return accountActions;
                    });
            }

            return Task.CompletedTask;
        }

        public Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId)
        {
            foreach (var actions in _actions.Where(x => x.Key.Item1 == blockchainType).Select(x => x.Value))
            {
                lock (actions)
                {
                    actions.RemoveAll(x => x.BlockId == blockId);
                }
            }

            return Task.CompletedTask;
        }

        public Task<Money> GetBalanceAsync(string blockchainType, Address address, Asset asset, long atBlockNumber)
        {
            throw new System.NotImplementedException();
        }

        public Task<IReadOnlyDictionary<Asset, Money>> GetBalancesAsync(string blockchainType, Address address, long atBlockNumber)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IReadOnlyDictionary<TransactionId, IReadOnlyDictionary<AccountId, Money>>> GetSomeOfBalancesAsync(
            string blockchainType,
            ISet<TransactionId> transactionIds)
        {
            var result = new Dictionary<TransactionId, Dictionary<AccountId, Money>>();
            var filtered = _actions
                .Where(x =>
                {
                    lock (x.Value)
                    {
                        return x.Key.Item1 == blockchainType &&
                               x.Value.Count(y => transactionIds.Contains(y.TransactionId)) != 0;
                    }
                })
                .ToArray();

            foreach (var item in filtered)
            {
                lock (item.Value)
                {
                    foreach (var accountAction in item.Value)
                    {
                        if (!result.TryGetValue(accountAction.TransactionId, out var accountMoneyDict))
                        {
                            result[accountAction.TransactionId] = new Dictionary<AccountId, Money>()
                            {
                                {accountAction.AccountId, accountAction.Amount}
                            };
                        }
                        else
                        {
                            accountMoneyDict[accountAction.AccountId] = accountAction.Amount;
                        }
                    }
                }
            }

            return result.ToDictionary(x => x.Key, y => (IReadOnlyDictionary<AccountId, Money>)y.Value);
        }
    }
}
