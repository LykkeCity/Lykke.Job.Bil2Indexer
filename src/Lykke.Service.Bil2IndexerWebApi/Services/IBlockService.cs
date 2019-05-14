using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public interface IBlockService
    {
        Task<BlockHeader> GetBlockByIdOrDefault(string blockchainType, BlockId id);
        Task<BlockHeader> GetBlockByNumberOrDefault(string blockchainType, int number);
        Task<IReadOnlyCollection<BlockHeader>> GetBlocks(string blockchainType, int limit, bool orderAsc, string startingAfter, string endingBefore);
        Task<BlockHeader> GetLastIrreversibleBlockAsync(string blockchainType);
        Task<BlockHeader> GetLastBlockAsync(string blockchainType);
    }
}
