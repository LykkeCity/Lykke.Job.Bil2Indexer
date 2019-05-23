using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public interface ITransactionQueryFacade
    {
        Task<TransactionResponce> GetTransactionById(string blockchainType, string id, IUrlHelper url);
        Task<IReadOnlyCollection<TransactionResponce>> GetTransactionsByBlockId(string blockchainType, string blockId, int limit, bool orderAsc,
            string startingAfter, string endingBefore, IUrlHelper url);
        Task<IReadOnlyCollection<TransactionResponce>> GetTransactionsByAddress(string blockchainType, string address, int limit, bool orderAsc,
            string startingAfter, string endingBefore, IUrlHelper url);
        Task<IReadOnlyCollection<TransactionResponce>> GetTransactionsByBlockNumber(string blockchainType, int blockNumberValue, int limit,
            bool orderAsc, string startingAfter, string endingBefore, IUrlHelper url);
    }
}
