using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface ITransactionsRepository
    {
        Task SaveAsync(string blockchainType, TransferAmountTransactionExecutedEvent transaction);
        Task SaveAsync(string blockchainType, TransferCoinsTransactionExecutedEvent transaction);
        Task SaveAsync(string blockchainType, TransactionFailedEvent transaction);
        Task<int> CountInBlockAsync(string blockchainType, string blockId);
        Task<PaginatedItems<TransferCoinsTransactionExecutedEvent>> GetTransferCoinsTransactionsOfBlockAsync(
            string blockchainType, 
            string blockId, 
            string continuation);
        Task<PaginatedItems<TransferAmountTransactionExecutedEvent>> GetTransferAmountTransactionsOfBlockAsync(
            string blockchainType, 
            string blockId, 
            string continuation);
        Task<PaginatedItems<TransactionFailedEvent>> GetFailedTransactionsOfBlockAsync(
            string blockchainType, 
            string blockId, 
            string continuation);
        Task<TransferCoinsTransactionExecutedEvent> GetTransferCoinsTransactionAsync(string blockchainType, string transactionId);
        Task<TransferAmountTransactionExecutedEvent> GetTransferAmountTransactionAsync(string blockchainType, string transactionId);
        Task<TransactionFailedEvent> GetFailedTransactionAsync(string blockchainType, string transactionId);
        Task<TransferCoinsTransactionExecutedEvent> GetTransferCoinsTransactionOrDefaultAsync(string blockchainType, string transactionId);
        Task<TransferAmountTransactionExecutedEvent> GetTransferAmountTransactionOrDefaultAsync(string blockchainType, string transactionId);
        Task<TransactionFailedEvent> GetFailedTransactionOrDefaultAsync(string blockchainType, string transactionId);
        Task TryRemoveAllOfBlockAsync(string blockchainType, string blockId);
    }
}
