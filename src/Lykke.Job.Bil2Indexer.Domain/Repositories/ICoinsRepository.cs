using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface ICoinsRepository
    {
        Task AddIfNotExistsAsync(IReadOnlyCollection<Coin> coins);
        Task SpendAsync(string blockchainType, IReadOnlyCollection<CoinId> ids);
        // TODO: By block id
        Task RevertSpendingAsync(string blockchainType, IReadOnlyCollection<CoinId> ids);
        Task<IReadOnlyCollection<Coin>> GetSomeOfAsync(string blockchainType, IReadOnlyCollection<CoinId> ids);
        Task RemoveIfExistAsync(string blockchainType, BlockId blockId);
        Task<IReadOnlyCollection<Coin>> GetUnspentAsync(string blockchainType,
            Address address, 
            int limit,
            bool orderAsc,
            CoinId startingAfter,
            CoinId endingBefore);
    }
}
