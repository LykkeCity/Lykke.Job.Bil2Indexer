using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public interface ITransactionQueryFacade
    {
        Task<TransactionModel> GetTransactionById(string blockchainType, string id);
        Task<Paginated<TransactionModel>> GetTransactionsByBlockId(string blockchainType, string blockId, int limit, bool orderAsc,
            string startingAfter, string endingBefore);
        Task<Paginated<TransactionModel>> GetTransactionsByAddress(string blockchainType, string address, int limit, bool orderAsc,
            string startingAfter, string endingBefore);
        Task<Paginated<TransactionModel>> GetTransactionsByBlockNumber(string blockchainType, int blockNumberValue, int limit,
            bool orderAsc, string startingAfter, string endingBefore);
    }
}
