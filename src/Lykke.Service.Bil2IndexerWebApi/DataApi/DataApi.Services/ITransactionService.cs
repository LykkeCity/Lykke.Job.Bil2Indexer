using System.Threading.Tasks;
using DataApi.Core.Domain;

namespace DataApi.Services
{
    public interface ITransactionService
    {
        Task<Transaction> GetTransactionById(string id);
        Task<Transaction[]> GetTransactionsByBlockId(string blockId, int limit, bool orderAsc, string startingAfter, string endingBefore);
        Task<Transaction[]> GetTransactionsByAddress(string address, int limit, bool orderAsc, string startingAfter, string endingBefore);
        Task<Transaction[]> GetTransactionsByBlockNumber(int blockNumberValue, int limit, bool orderAsc, string startingAfter, string endingBefore);
    }
}
