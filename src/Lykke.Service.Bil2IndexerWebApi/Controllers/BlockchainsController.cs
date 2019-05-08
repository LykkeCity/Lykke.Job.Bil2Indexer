using Lykke.Service.Bil2IndexerWebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Controllers
{
    [Route("api/blockchains")]
    [ApiController]
    public class BlockchainsController : ControllerBase
    {
        [HttpGet(Name = nameof(GetBlockchains))]
        public ActionResult<BlockchainModel[]> GetBlockchains()
        {
            return Ok(new[]
            {
                new BlockchainModel {BlockchainType = "Bitcoin"},
                new BlockchainModel {BlockchainType = "Ripple"}
            });
        }
    }
}
