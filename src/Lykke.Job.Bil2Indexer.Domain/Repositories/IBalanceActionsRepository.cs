using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface IBalanceActionsRepository
    {
        Task SaveAsync(string blockchainType, IEnumerable<BalanceAction> actions);
        Task TryRemoveAllOfBlockAsync(string blockchainType, string blockId);
        Task<Money> GetBalanceAsync(string blockchainType, Address address, Asset asset, long atBlockNumber);
        Task<IReadOnlyDictionary<Asset, Money>> GetBalancesAsync(string blockchainType, Address address, long atBlockNumber);
    }
}
