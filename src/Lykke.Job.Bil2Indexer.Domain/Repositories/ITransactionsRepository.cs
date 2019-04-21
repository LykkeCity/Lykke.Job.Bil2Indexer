using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface ITransactionsRepository
    {
        Task AddIfNotExistsAsync(string blockchainType, TransferAmountTransactionExecutedEvent transaction);
        Task AddIfNotExistsAsync(string blockchainType, TransferCoinsTransactionExecutedEvent transaction);
        Task AddIfNotExistsAsync(string blockchainType, TransactionFailedEvent transaction);
        Task<int> CountInBlockAsync(string blockchainType, BlockId blockId);
        Task<PaginatedItems<TransactionEnvelope>> GetAllOfBlockAsync(
            string blockchainType, 
            BlockId blockId, 
            int limit,
            string continuation);
        Task<TransactionEnvelope> GetAsync(string blockchainType, TransactionId transactionId);
        Task<TransactionEnvelope> GetOrDefaultAsync(string blockchainType, TransactionId transactionId);
        Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId);
    }
}
