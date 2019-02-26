using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;

namespace Lykke.Job.Bil2Indexer.Domain.Services
{
    public interface ITransactionsProcessor
    {
        Task ProcessExecutedTransactionAsync(TransactionExecutedEvent transaction);
        Task ProcessFailedTransactionAsync(TransactionFailedEvent transaction);
    }
}