using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public interface IBlockService
    {
        Task<BlockHeader> GetBlockByIdOrDefault(BlockId id);
        Task<BlockHeader> GetBlockByNumberOrDefault(int number);
        Task<BlockHeader[]> GetBlocks(int limit, bool orderAsc, string startingAfter, string endingBefore);
    }
}
