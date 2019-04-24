using System.Threading.Tasks;
using DataApi.Core.Domain;

namespace DataApi.Services
{
    public class TransactionService : ITransactionService
    {
        public Task<Transaction> GetTransactionById(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task<Transaction[]> GetTransactionsByBlockId(string blockId, int limit, bool orderAsc, string startingAfter, string endingBefore)
        {
            throw new System.NotImplementedException();
        }

        public Task<Transaction[]> GetTransactionsByAddress(string address, int limit, bool orderAsc, string startingAfter, string endingBefore)
        {
            throw new System.NotImplementedException();
        }

        public Task<Transaction[]> GetTransactionsByBlockNumber(int blockNumberValue, int limit, bool orderAsc, string startingAfter,
            string endingBefore)
        {
            throw new System.NotImplementedException();
        }

        public Task<Transaction[]> GetTransactionsByBlockId(int limit, bool orderAsc, string startingAfter, string endingBefore)
        {
            throw new System.NotImplementedException();
        }
    }
}
