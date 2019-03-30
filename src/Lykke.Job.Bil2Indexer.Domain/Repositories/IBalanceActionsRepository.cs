using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface IBalanceActionsRepository
    {
        Task SaveAsync(string blockchainType, IEnumerable<BalanceAction> actions);
    }
}
