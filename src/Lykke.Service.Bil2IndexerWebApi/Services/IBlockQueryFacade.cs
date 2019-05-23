using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public interface IBlockQueryFacade
    {
        Task<BlockResponce> GetBlockByIdOrDefault(string blockchainType, BlockId id);
        Task<BlockResponce> GetBlockByNumberOrDefault(string blockchainType, long number);
        Task<IReadOnlyCollection<BlockResponce>> GetBlocks(string blockchainType, int limit, bool orderAsc, long? startingAfter, long? endingBefore);
        Task<BlockResponce> GetLastIrreversibleBlockAsync(string blockchainType);
        Task<BlockResponce> GetLastBlockAsync(string blockchainType);
    }
}
