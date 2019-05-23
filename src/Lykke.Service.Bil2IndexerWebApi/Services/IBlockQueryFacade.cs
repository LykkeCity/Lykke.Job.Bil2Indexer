using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public interface IBlockQueryFacade
    {
        Task<BlockResponce> GetBlockByIdOrDefault(string blockchainType, BlockId id, IUrlHelper url);
        Task<BlockResponce> GetBlockByNumberOrDefault(string blockchainType, long number, IUrlHelper url);
        Task<IReadOnlyCollection<BlockResponce>> GetBlocks(string blockchainType, int limit, bool orderAsc, long? startingAfter, long? endingBefore, IUrlHelper url);
        Task<BlockResponce> GetLastIrreversibleBlockAsync(string blockchainType, IUrlHelper url);
        Task<BlockResponce> GetLastBlockAsync(string blockchainType, IUrlHelper url);
    }
}
