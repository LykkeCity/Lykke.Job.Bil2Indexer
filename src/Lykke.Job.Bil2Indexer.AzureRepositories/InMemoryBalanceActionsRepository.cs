using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.Common;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.AzureRepositories
{
    public class InMemoryBalanceActionsRepository : IBalanceActionsRepository
    {
        private readonly ConcurrentDictionary<(string, Address, Asset), List<BalanceAction>> _actions;

        public InMemoryBalanceActionsRepository()
        {
            _actions = new ConcurrentDictionary<(string, Address, Asset), List<BalanceAction>>();
        }

        public Task SaveAsync(string blockchainType, IEnumerable<BalanceAction> actions)
        {
            foreach (var action in actions)
            {
                _actions.AddOrUpdate(
                    (blockchainType, action.Address, action.Asset),
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

        public Task TryRemoveAllOfBlockAsync(string blockchainType, string blockId)
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
    }
}
