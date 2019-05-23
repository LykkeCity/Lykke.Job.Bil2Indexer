using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public interface ITransactionQueryFacade
    {
        Task<TransactionResponce> GetTransactionById(string blockchainType, string id);
        Task<IReadOnlyCollection<TransactionResponce>> GetTransactionsByBlockId(string blockchainType, string blockId, int limit, bool orderAsc,
            string startingAfter, string endingBefore);
        Task<IReadOnlyCollection<TransactionResponce>> GetTransactionsByAddress(string blockchainType, string address, int limit, bool orderAsc,
            string startingAfter, string endingBefore);
        Task<IReadOnlyCollection<TransactionResponce>> GetTransactionsByBlockNumber(string blockchainType, int blockNumberValue, int limit,
            bool orderAsc, string startingAfter, string endingBefore);
    }
}
