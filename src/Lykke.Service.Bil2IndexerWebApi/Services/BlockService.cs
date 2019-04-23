using System.Threading.Tasks;
using DataApi.Core.Domain;

namespace DataApi.Services
{
    public class BlockService : IBlockService
    {
        public Task<Block> GetBlockById(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task<Block> GetBlockByNumber(int number)
        {
            throw new System.NotImplementedException();
        }

        public Task<Block[]> GetBlocks(int limit, bool orderAsc, string startingAfter, string endingBefore)
        {
            throw new System.NotImplementedException();
        }
    }
}
