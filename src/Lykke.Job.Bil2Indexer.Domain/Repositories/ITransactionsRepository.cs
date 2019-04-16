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
        Task<PaginatedItems<TransferCoinsTransactionExecutedEvent>> GetTransferCoinsTransactionsOfBlockAsync(
            string blockchainType, 
            BlockId blockId, 
            int limit,
            string continuation);
        Task<PaginatedItems<TransferAmountTransactionExecutedEvent>> GetTransferAmountTransactionsOfBlockAsync(
            string blockchainType, 
            BlockId blockId, 
            int limit,
            string continuation);
        Task<PaginatedItems<TransactionFailedEvent>> GetFailedTransactionsOfBlockAsync(
            string blockchainType, 
            BlockId blockId, 
            int limit,
            string continuation);
        Task<TransferCoinsTransactionExecutedEvent> GetTransferCoinsTransactionAsync(string blockchainType, TransactionId transactionId);
        Task<TransferAmountTransactionExecutedEvent> GetTransferAmountTransactionAsync(string blockchainType, TransactionId transactionId);
        Task<TransactionFailedEvent> GetFailedTransactionAsync(string blockchainType, TransactionId transactionId);
        Task<TransferCoinsTransactionExecutedEvent> GetTransferCoinsTransactionOrDefaultAsync(string blockchainType, TransactionId transactionId);
        Task<TransferAmountTransactionExecutedEvent> GetTransferAmountTransactionOrDefaultAsync(string blockchainType, TransactionId transactionId);
        Task<TransactionFailedEvent> GetFailedTransactionOrDefaultAsync(string blockchainType, TransactionId transactionId);
        Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId);
    }
}
