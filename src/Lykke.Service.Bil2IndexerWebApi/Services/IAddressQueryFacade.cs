using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public interface IAddressQueryFacade
    {
        Task<IReadOnlyCollection<AddressUnspentOutputModel>> GetUnspentOutputs(
            string blockchainType, 
            string address,
            int limit,
            bool orderAsc, 
            string startingAfter,
            string endingBefore);

        Task<IReadOnlyCollection<AddressBalanceModel>> GetBalances(
            string blockchainType, 
            string address, 
            int limit, 
            bool orderAsc,
            string startingAfter,
            string endingBefore);

        Task<IReadOnlyCollection<AddressBalanceModel>> GetBalancesByBlockId(
            string blockchainType, 
            string address,
            string blockId,
            int limit, 
            bool orderAsc,
            string startingAfter,
            string endingBefore);

        Task<IReadOnlyCollection<AddressBalanceModel>> GetBalancesByBlockNumber(
            string blockchainType, 
            string address,
            int blockNumber,
            int limit,
            bool orderAsc,
            string startingAfter,
            string endingBefore);

        Task<IReadOnlyCollection<AddressBalanceModel>> GetBalancesOnDate(string blockchainType,
            string address,
            DateTime date,
            int limit,
            bool orderAsc,
            string startingAfter,
            string endingBefore);
    }
}
