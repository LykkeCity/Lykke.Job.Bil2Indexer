using DataApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlockchainsController : ControllerBase
    {
        [HttpGet(Name = nameof(GetBlockchains))]
        public ActionResult<BlockchainModel[]> GetBlockchains()
        {
            return Ok(new[]
            {
                new BlockchainModel {Id = "Bitcoin", IndexerUrl = "https://bitcoin.data.qoob.tech"},
                new BlockchainModel {Id = "Ripple", IndexerUrl = "https://ripple.data.qoob.tech"}
            });
        }
    }
}