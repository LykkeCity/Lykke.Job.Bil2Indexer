using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Numerics;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public interface IAddressQueryFacade
    {
        Task<Paginated<AddressBalanceModel>> GetUnspentOutputs(
            string blockchainType, 
            string address,
            int limit,
            bool orderAsc, 
            string startingAfter,
            string endingBefore);

        Task<Paginated<AddressBalanceModel>> GetBalances(
            string blockchainType, 
            string address, 
            int limit, 
            bool orderAsc,
            string startingAfter,
            string endingBefore);

        Task<IReadOnlyDictionary<Address, IReadOnlyDictionary<Asset, Money>>> GetBalancesByBlockId(
            string blockchainType, 
            string address,
            string blockId,
            int limit, 
            bool orderAsc,
            string startingAfter,
            string endingBefore);

        Task<IReadOnlyDictionary<Address, IReadOnlyDictionary<Asset, Money>>> GetBalancesByBlockNumber(
            string blockchainType, 
            string address,
            int blockNumber,
            int limit,
            bool orderAsc,
            string startingAfter,
            string endingBefore);

        Task<IReadOnlyDictionary<Address, IReadOnlyDictionary<Asset, Money>>> GetBalancesOnDate(string blockchainType,
            string address,
            DateTime date,
            int limit,
            bool orderAsc,
            string startingAfter,
            string endingBefore);
    }
}
