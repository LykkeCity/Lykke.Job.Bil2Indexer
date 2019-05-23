using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public interface IBlockQueryFacade
    {
        Task<BlockModel> GetBlockByIdOrDefault(string blockchainType, BlockId id);
        Task<BlockModel> GetBlockByNumberOrDefault(string blockchainType, long number);
        Task<IReadOnlyCollection<BlockModel>> GetBlocks(string blockchainType, int limit, bool orderAsc, long? startingAfter, long? endingBefore);
        Task<BlockModel> GetLastIrreversibleBlockAsync(string blockchainType);
        Task<BlockModel> GetLastBlockAsync(string blockchainType);
    }
}
