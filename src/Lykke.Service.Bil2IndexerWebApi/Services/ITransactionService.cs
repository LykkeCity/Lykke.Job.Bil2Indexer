using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public interface ITransactionService
    {
        Task<Transaction> GetTransactionById(string blockchainType, string id);
        Task<IReadOnlyCollection<Transaction>> GetTransactionsByBlockId(string blockchainType, string blockId, int limit, bool orderAsc,
            string startingAfter, string endingBefore);
        Task<IReadOnlyCollection<Transaction>> GetTransactionsByAddress(string blockchainType, string address, int limit, bool orderAsc,
            string startingAfter, string endingBefore);
        Task<IReadOnlyCollection<Transaction>> GetTransactionsByBlockNumber(string blockchainType, int blockNumberValue, int limit,
            bool orderAsc, string startingAfter, string endingBefore);
    }
}
