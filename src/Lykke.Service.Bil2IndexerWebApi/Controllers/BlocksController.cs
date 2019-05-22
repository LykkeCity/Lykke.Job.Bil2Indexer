using System;
using System.Threading.Tasks;
using Lykke.Service.Bil2IndexerWebApi.Mappers;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;
using Lykke.Service.Bil2IndexerWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Controllers
{
    [Route("api/blockchains/{blockchainType}/blocks")]
    public class BlocksController : ControllerBase
    {
        private readonly IBlockQueryFacade _blockQueryFacade;

        public BlocksController(IBlockQueryFacade blockQueryFacade)
        {
            _blockQueryFacade = blockQueryFacade;
        }

        [HttpGet(Name = nameof(GetBlocks))]
        public async Task<ActionResult<Paginated<BlockModel, long>>> GetBlocks(
            [FromRoute] string blockchainType,
            [FromQuery] int? number,
            [FromQuery] PaginationRequest<long> pagination)
        {
            // TODO: Validate parameters

            if (number != null)
            {
                var block = await _blockQueryFacade.GetBlockByNumberOrDefault(blockchainType, number.Value);

                if (block == null)
                {
                    return NotFound();
                }

                return block.PaginateSingle(pagination);
            }

            var blocks = await _blockQueryFacade.GetBlocks(blockchainType, pagination.Limit, 
                pagination.Order == PaginationOrder.Asc,
                pagination.StartingAfter,
                pagination.EndingBefore);

            return blocks.Paginate(pagination);
        }

        [HttpGet("/{id}", Name = nameof(GetBlockById))]
        public async Task<ActionResult<BlockModel>> GetBlockById(
            [FromRoute] string blockchainType,
            [FromRoute] string id)
        {
            var block = await _blockQueryFacade.GetBlockByIdOrDefault(blockchainType, id);

            if (block == null)
            {
                return NotFound();
            }
            
            return block;
        }

        [HttpGet("/last-irreversible", Name = nameof(GetIrreversibleBlock))]
        public async Task<ActionResult<BlockModel>> GetIrreversibleBlock([FromRoute] string blockchainType)
        {
            var block = await _blockQueryFacade.GetLastIrreversibleBlockAsync(blockchainType);

            if (block == null)
            {
                return NotFound();
            }

            return block;
        }

        [HttpGet("/last", Name = nameof(GetLastBlock))]
        public async Task<ActionResult<BlockModel>> GetLastBlock([FromRoute] string blockchainType)
        {
            var block = await _blockQueryFacade.GetLastBlockAsync(blockchainType);

            if (block == null)
            {
                return NotFound();
            }

            return block;
        }

        [HttpGet("/{id}/raw", Name = nameof(GetRawBlock))]
        public IActionResult GetRawBlock(
            [FromRoute] string blockchainType,
            [FromRoute] string id)
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
