using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public interface IAddressQueryFacade
    {
        Task<IReadOnlyCollection<AddressUnspentOutputResponce>> GetUnspentOutputs(
            string blockchainType, 
            string address,
            int limit,
            bool orderAsc, 
            string startingAfter,
            string endingBefore);

        Task<IReadOnlyCollection<AddressBalanceResponce>> GetBalances(
            string blockchainType, 
            string address);

        Task<IReadOnlyCollection<AddressBalanceResponce>> GetBalancesByBlockId(
            string blockchainType, 
            string address,
            string blockId);

        Task<IReadOnlyCollection<AddressBalanceResponce>> GetBalancesByBlockNumber(
            string blockchainType, 
            string address,
            long blockNumber);
    
        Task<IReadOnlyCollection<AddressBalanceResponce>> GetBalancesOnDate(string blockchainType,
            string address,
            DateTime date);
    }
}
