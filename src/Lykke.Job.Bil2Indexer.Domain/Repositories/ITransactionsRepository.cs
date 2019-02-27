using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface ITransactionsRepository
    {
        Task SaveAsync(TransactionExecutedEvent transaction);
        Task SaveAsync(TransactionFailedEvent transaction);
    }
}
