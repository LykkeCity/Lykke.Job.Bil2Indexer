using System.Linq;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Controllers
{
    [Route("api/blockchains")]
    public class BlockchainsController : ControllerBase
    {
        [HttpGet(Name = nameof(GetBlockchains))]
        public ActionResult<BlockchainResponce[]> GetBlockchains()
        {
            return Ok(SupportedBlockchains.List.Select(blockchainType=> new BlockchainResponce
            {
                BlockchainType = blockchainType
            }).ToList());
        }
    }
}
