using System;
using System.Threading.Tasks;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public class AddressService : IAddressService
    {
        public Task<UnspentOutput[]> GetUnspentOutputs(int limit, bool orderAsc, string startingAfter, string endingBefore)
        {
            throw new NotImplementedException();
        }

        public Task<UnspentOutput[]> GetUnspentOutputs(string address, int limit, bool orderAsc, string startingAfter, string endingBefore)
        {
            throw new NotImplementedException();
        }

        public Task<Balance[]> GetBalancesByAddress(string address, int limit, bool orderAsc, string startingAfter, string endingBefore)
        {
            throw new NotImplementedException();
        }

        public Task<Balance[]> GetBalancesByBlockId(string blockId, int limit, bool orderAsc, string startingAfter, string endingBefore)
        {
            throw new NotImplementedException();
        }

        public Task<Balance[]> GetBalancesByBlockNumber(int blockNumber, int limit, bool orderAsc, string startingAfter, string endingBefore)
        {
            throw new NotImplementedException();
        }

        public Task<Balance[]> GetBalancesOnDate(DateTime date, int limit, bool orderAsc, string startingAfter, string endingBefore)
        {
            throw new NotImplementedException();
        }
    }
}
