using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.Common;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.AzureRepositories
{
    public class InMemoryBalanceActionsRepository : IBalanceActionsRepository
    {
        private readonly ConcurrentDictionary<(string, Address, Asset), ConcurrentBag<BalanceAction>> _actions;

        public InMemoryBalanceActionsRepository()
        {
            _actions = new ConcurrentDictionary<(string, Address, Asset), ConcurrentBag<BalanceAction>>();
        }

        public Task SaveAsync(string blockchainType, IEnumerable<BalanceAction> actions)
        {
            foreach (var action in actions)
            {
                _actions.AddOrUpdate(
                    (blockchainType, action.Address, action.Asset),
                    key => new ConcurrentBag<BalanceAction>
                    {
                        action
                    },
                    (key, accountActions) =>
                    {
                        accountActions.Add(action);

                        return accountActions;
                    });
            }

            return Task.CompletedTask;
        }
    }
}
