using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Numerics;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public interface IAddressService
    {
        Task<IReadOnlyCollection<Coin>> GetUnspentOutputs(
            string blockchainType, 
            string address,
            int limit,
            bool orderAsc, 
            string startingAfter,
            string endingBefore);

        Task<IReadOnlyDictionary<Asset, Money>> GetBalances(
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
