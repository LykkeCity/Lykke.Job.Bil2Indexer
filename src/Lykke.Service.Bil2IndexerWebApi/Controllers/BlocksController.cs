using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.Bil2IndexerWebApi.Factories;
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
        private readonly IBlockService _blockService;
        private readonly IBlockModelFactory _blockModelFactory;

        public BlocksController(IBlockService blockService, IBlockModelFactory blockModelFactory)
        {
            _blockService = blockService;
            _blockModelFactory = blockModelFactory;
        }

        [HttpGet(Name = nameof(GetBlocks))]
        public async Task<ActionResult<Paginated<BlockModel[]>>> GetBlocks(
            [FromRoute] string blockchainType,
            [FromQuery] int? number,
            [FromQuery] DateTimeOffset? datetime,
            PaginationOrder order,
            string startingAfter, 
            string endingBefore, 
            int limit = 25)
        {
            Paginated<BlockModel[]> model;
            
            if (number != null)
            {
                var block = await _blockService.GetBlockByNumberOrDefault(number.Value);

                if (block == null)
                {
                    return NotFound();
                }

                model = _blockModelFactory.PrepareBlocksPaginated(new[] { block });

                return model;
            }

            var blocks = await _blockService.GetBlocks(limit, order == PaginationOrder.Asc, startingAfter, endingBefore);

            model = _blockModelFactory.PrepareBlocksPaginated(blocks);

            return model;
        }

        [HttpGet("/{id}", Name = nameof(GetBlockById))]
        public async Task<ActionResult<BlockModel>> GetBlockById(
            [FromRoute] string blockchainType,
            [FromRoute] string id)
        {
            var block = await _blockService.GetBlockByIdOrDefault(id);

            if (block == null)
            {
                return NotFound();
            }

            var model = _blockModelFactory.PrepareBlockModel(block);

            return model;
        }

        [HttpGet("/last-irreversible", Name = nameof(GetBlockById))]
        public async Task<ActionResult<BlockModel>> GetIrreversibleBlock([FromRoute] string blockchainType)
        {
            var block = await _blockService.GetBlockByIdOrDefault(id);

            if (block == null)
            {
                return NotFound();
            }

            var model = _blockModelFactory.PrepareBlockModel(block);

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

            string test = "Some big block raw contents";

            // convert string to stream
            byte[] byteArray = Encoding.ASCII.GetBytes(test);
            MemoryStream stream = new MemoryStream(byteArray);

            return File(stream, "application/octet-stream", $"{id}.txt");
        }
    }
}
