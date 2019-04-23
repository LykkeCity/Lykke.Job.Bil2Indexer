using System.Threading.Tasks;
using DataApi.Core.Domain;

namespace DataApi.Services
{
    public interface IBlockService
    {
        Task<Block> GetBlockById(string id);
        Task<Block> GetBlockByNumber(int number);
        Task<Block[]> GetBlocks(int limit, bool orderAsc, string startingAfter, string endingBefore);
    }
}
