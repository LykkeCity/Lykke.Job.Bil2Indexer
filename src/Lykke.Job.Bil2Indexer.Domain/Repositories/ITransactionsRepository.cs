using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface ITransactionsRepository
    {
        Task AddIfNotExistsAsync(IReadOnlyCollection<Transaction> transactions);
        Task<int> CountInBlockAsync(string blockchainType, BlockId blockId);
        Task<PaginatedItems<Transaction>> GetAllOfBlockAsync(
            string blockchainType, 
            BlockId blockId, 
            int limit,
            string continuation);
        Task<Transaction> GetAsync(string blockchainType, TransactionId transactionId);
        Task<Transaction> GetOrDefaultAsync(string blockchainType, TransactionId transactionId);
        Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId);
    }
}
