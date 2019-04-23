using System.IO;
using System.Text;
using System.Threading.Tasks;
using DataApi.Factories;
using DataApi.Models;
using DataApi.Models.Common;
using DataApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("api/[controller]")]
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

        [HttpGet("{id}", Name = nameof(GetBlockById))]
        public async Task<ActionResult<BlockModel>> GetBlockById([FromRoute] string id)
        {
            var block = await _blockService.GetBlockById(id);

            var model = _blockModelFactory.PrepareBlockModel(block);

            return model;
        }

        [HttpGet(Name = nameof(GetBlocks))]
        public async Task<ActionResult<Paginated<BlockModel[]>>> GetBlocks([FromQuery] int? number,
            PaginationOrder order, string startingAfter, string endingBefore, int limit = 25)
        {
            Paginated<BlockModel[]> model;
            if (number != null)
            {
                var block = await _blockService.GetBlockByNumber(number.Value);

                model = _blockModelFactory.PrepareBlocksPaginated(new[] { block });

                return model;
            }

            var blocks =
                await _blockService.GetBlocks(limit, order == PaginationOrder.Asc, startingAfter, endingBefore);

            model = _blockModelFactory.PrepareBlocksPaginated(blocks);

            return model;
        }

        [HttpGet("{id}/raw", Name = nameof(GetRawBlock))]
        public IActionResult GetRawBlock(string id)
        {
            if (id == null)
                return NotFound();

            string test = "Some big block raw contents";

            // convert string to stream
            byte[] byteArray = Encoding.ASCII.GetBytes(test);
            MemoryStream stream = new MemoryStream(byteArray);

            return File(stream, "application/octet-stream", $"{id}.txt");
        }
    }
}