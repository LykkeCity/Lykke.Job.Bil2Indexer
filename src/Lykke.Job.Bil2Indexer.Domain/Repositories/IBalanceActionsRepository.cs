using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Contract.Events;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface IBalanceActionsRepository
    {
        Task AddIfNotExistsAsync(string blockchainType, IEnumerable<BalanceAction> actions);
        Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId);
        Task<Money> GetBalanceAsync(string blockchainType, Address address, Asset asset, long atBlockNumber);
        Task<IReadOnlyDictionary<Asset, Money>> GetBalancesAsync(string blockchainType, Address address, long atBlockNumber);
        Task<IReadOnlyDictionary<TransactionId, IReadOnlyDictionary<AccountId, Money>>> GetBalancesAsync(
            string blockchainType, 
            ISet<TransactionId> transactionIds, 
            long atBlockNumber);
    }
}
