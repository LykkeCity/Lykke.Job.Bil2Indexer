using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface ICoinsRepository
    {
        Task AddIfNotExistsAsync(IEnumerable<Coin> coins);
        Task SpendAsync(string blockchainType, IReadOnlyCollection<CoinId> ids);
        Task RevertSpendingAsync(string blockchainType, IReadOnlyCollection<CoinId> ids);
        Task<IReadOnlyCollection<Coin>> GetSomeOfAsync(string blockchainType, IReadOnlyCollection<CoinId> ids);
        Task RemoveIfExistAsync(string blockchainType, ISet<TransactionId> receivedInTransactionIds);
    }
}
