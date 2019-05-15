using System.IO;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain.Extensions;
using Lykke.Service.Bil2IndexerWebApi.Mappers;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;
using Lykke.Service.Bil2IndexerWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Controllers
{
    [Route("api/blockchains/{blockchainType}/blocks")]
    [ApiController]
    public class BlocksController : ControllerBase
    {
        private readonly IBlockQueryFacade _blockQueryFacade;

        public BlocksController(IBlockQueryFacade blockQueryFacade)
        {
            _blockQueryFacade = blockQueryFacade;
        }

        [HttpGet(Name = nameof(GetBlocks))]
        public async Task<ActionResult<Paginated<BlockModel>>> GetBlocks(
            [FromRoute] string blockchainType,
            [FromQuery] int? number,
            [FromQuery] PaginationOrder order,
            [FromQuery] string startingAfter, 
            [FromQuery] string endingBefore, 
            [FromQuery] int limit = 25)
        {
            // TODO: Validate parameters

            if (number != null)
            {
                var block = await _blockQueryFacade.GetBlockByNumberOrDefault(blockchainType, number.Value);

                if (block == null)
                {
                    return NotFound();
                }

                return BlockModelMapper.Map(new[] { block });
            }

            var blocks = await _blockQueryFacade.GetBlocks(blockchainType, limit, order == PaginationOrder.Asc, startingAfter, endingBefore);

            return BlockModelMapper.Map(blocks);
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

            var model = BlockModelMapper.Map(block);

            return model;
        }

        [HttpGet("/last-irreversible", Name = nameof(GetIrreversibleBlock))]
        public async Task<ActionResult<BlockModel>> GetIrreversibleBlock([FromRoute] string blockchainType)
        {
            var block = await _blockQueryFacade.GetLastIrreversibleBlockAsync(blockchainType);

            if (block == null)
            {
                return NotFound();
            }

            var model = BlockModelMapper.Map(block);

            return model;
        }

        [HttpGet("/last", Name = nameof(GetLastBlock))]
        public async Task<ActionResult<BlockModel>> GetLastBlock([FromRoute] string blockchainType)
        {
            var block = await _blockQueryFacade.GetLastBlockAsync(blockchainType);

            if (block == null)
            {
                return NotFound();
            }

            var model = BlockModelMapper.Map(block);

            return model;
        }

        [HttpGet("/{id}/raw", Name = nameof(GetRawBlock))]
        public IActionResult GetRawBlock(
            [FromRoute] string blockchainType,
            [FromRoute] string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var test = "Some big block raw contents".ToBase64();

            var stream = new MemoryStream(test.DecodeToBytes());
            
            return File(stream, "application/octet-stream", $"{blockchainType}-{id}");
        }
    }
}
