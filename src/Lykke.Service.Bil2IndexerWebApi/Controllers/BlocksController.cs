using Lykke.Service.Bil2IndexerWebApi.Mappers;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests.Shared;
using Lykke.Service.Bil2IndexerWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Lykke.Service.Bil2IndexerWebApi.Controllers
{
    public class BlocksController : ControllerBase
    {
        private const string RoutePrefix = "api/blockchains/{blockchainType}/blocks";
        private readonly IBlockQueryFacade _blockQueryFacade;

        public BlocksController(IBlockQueryFacade blockQueryFacade)
        {
            _blockQueryFacade = blockQueryFacade;
        }

        [HttpGet(RoutePrefix, Name = nameof(GetBlocks))]
        public async Task<ActionResult<Paginated<BlockResponce, long>>> GetBlocks(
            [FromRoute][FromQuery] BlocksRequest request)
        {
            var blocks = await _blockQueryFacade.GetBlocks(request.BlockchainType, request.Pagination.Limit,
                request.Pagination.Order == PaginationOrder.Asc,
                request.Pagination.StartingAfter,
                request.Pagination.EndingBefore, 
                Url);

            return blocks.Paginate(request.Pagination, Url, p => p.Number);
        }

        [HttpGet(RoutePrefix + "/{id}", Name = nameof(GetBlockById))]
        public async Task<ActionResult<BlockResponce>> GetBlockById(
            [FromRoute] string blockchainType,
            [FromRoute] string id)
        {
            return await _blockQueryFacade.GetBlockByIdOrDefault(blockchainType, id, Url);
        }

        [HttpGet(RoutePrefix + "/{height:int}", Name = nameof(GetBlockByHeigh))]
        public async Task<ActionResult<BlockResponce>> GetBlockByHeigh([FromRoute] ByBlockNumberRequest request)
        {
            return await _blockQueryFacade.GetBlockByNumberOrDefault(request.BlockchainType, request.Number, Url);
        }

        [HttpGet(RoutePrefix + "/last-irreversible", Name = nameof(GetIrreversibleBlock))]
        public async Task<ActionResult<BlockResponce>> GetIrreversibleBlock([FromRoute] ByBlockchainRequest request)
        {
            return await _blockQueryFacade.GetLastIrreversibleBlockAsync(request.BlockchainType, Url);
        }

        [HttpGet(RoutePrefix + "/last", Name = nameof(GetLastBlock))]
        public async Task<ActionResult<BlockResponce>> GetLastBlock([FromRoute] ByBlockchainRequest request)
        {
            return await _blockQueryFacade.GetLastBlockAsync(request.BlockchainType, Url);
        }

        [HttpGet(RoutePrefix+ "/{id}/raw", Name = nameof(GetRawBlock))]
        public IActionResult GetRawBlock(
            [FromRoute] ByIdRequest request)
        {
            throw new NotImplementedException();
            //if (id == null)
            //{
            //    return NotFound();
            //}

            //var test = "Some big block raw contents".ToBase64();

            //var stream = new MemoryStream(test.DecodeToBytes());
            
            //return File(stream, "application/octet-stream", $"{blockchainType}-{id}");
        }
    }
}
