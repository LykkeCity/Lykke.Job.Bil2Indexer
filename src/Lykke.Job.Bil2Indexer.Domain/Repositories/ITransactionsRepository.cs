using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface ITransactionsRepository
    {
        Task SaveAsync(TransferAmountTransactionExecutedEvent transaction);
        Task SaveAsync(TransferCoinsTransactionExecutedEvent transaction);
        Task SaveAsync(TransactionFailedEvent transaction);
    }
}
