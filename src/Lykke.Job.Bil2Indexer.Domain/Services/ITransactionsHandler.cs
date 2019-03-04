using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;

namespace Lykke.Job.Bil2Indexer.Domain.Services
{
    public interface ITransactionsHandler
    {
        Task ProcessExecutedTransactionAsync(TransferAmountTransactionExecutedEvent transaction);
        Task ProcessExecutedTransactionAsync(TransferCoinsTransactionExecutedEvent transaction);
        Task ProcessFailedTransactionAsync(TransactionFailedEvent transaction);
    }
}
