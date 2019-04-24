using System;
using System.Threading.Tasks;
using DataApi.Core.Domain;

namespace DataApi.Services
{
    public interface IAddressService
    {
        Task<UnspentOutput[]> GetUnspentOutputs(string address, int limit, bool orderAsc, string startingAfter,
            string endingBefore);

        Task<Balance[]> GetBalancesByAddress(string address, int limit, bool orderAsc, string startingAfter,
            string endingBefore);

        Task<Balance[]> GetBalancesByBlockId(string blockId, int limit, bool orderAsc, string startingAfter,
            string endingBefore);

        Task<Balance[]> GetBalancesByBlockNumber(int blockNumber, int limit, bool orderAsc, string startingAfter,
            string endingBefore);

        Task<Balance[]> GetBalancesOnDate(DateTime date, int limit, bool orderAsc, string startingAfter,
            string endingBefore);
    }
}
