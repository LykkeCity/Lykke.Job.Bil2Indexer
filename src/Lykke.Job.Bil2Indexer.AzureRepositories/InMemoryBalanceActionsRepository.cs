using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Contract.Events;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.AzureRepositories
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

        public Task<IReadOnlyDictionary<TransactionId, IReadOnlyDictionary<AccountId, Money>>> GetBalancesAsync(string blockchainType, ISet<TransactionId> transactionIds, long atBlockNumber)
        {
            throw new System.NotImplementedException();
        }
    }
}
