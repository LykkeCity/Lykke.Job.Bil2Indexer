using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.Common;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface ICoinsRepository
    {
        Task SaveAsync(IEnumerable<Coin> coins);

        Task<IReadOnlyCollection<Coin>> GetSomeOfAsync(string blockchainType, IEnumerable<CoinReference> ids);

        Task<IReadOnlyCollection<Coin>> GetReceivedInTransactionAsync(string blockchainType, string transactionId);
		
        Task TryRemoveReceivedInTransactionAsync(string blockchainType, string transactionId);
    }
}
