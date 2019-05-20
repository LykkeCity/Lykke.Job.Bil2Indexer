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
            string address);

        Task<IReadOnlyCollection<AddressBalanceModel>> GetBalancesByBlockId(
            string blockchainType, 
            string address,
            string blockId);

        Task<IReadOnlyCollection<AddressBalanceModel>> GetBalancesByBlockNumber(
            string blockchainType, 
            string address,
            long blockNumber);
    
        Task<IReadOnlyCollection<AddressBalanceModel>> GetBalancesOnDate(string blockchainType,
            string address,
            DateTime date);
    }
}
