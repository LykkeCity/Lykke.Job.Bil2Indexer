using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.Common;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface ICoinsRepository
    {
        Task SaveAsync(IEnumerable<Coin> coins);
        Task SpendAsync(string blockchainType, IEnumerable<CoinReference> ids);
        Task RevertSpendingAsync(string blockchainType, IEnumerable<CoinReference> ids);
        Task<IReadOnlyCollection<Coin>> GetSomeOfAsync(string blockchainType, IEnumerable<CoinReference> ids);
        Task RemoveIfExistAsync(string blockchainType, IEnumerable<string> receivedInTransactionIds);
    }
}
