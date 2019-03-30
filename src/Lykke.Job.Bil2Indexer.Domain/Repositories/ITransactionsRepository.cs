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
    }
}
