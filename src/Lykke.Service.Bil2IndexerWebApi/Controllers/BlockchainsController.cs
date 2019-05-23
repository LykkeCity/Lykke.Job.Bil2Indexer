using System.Linq;
using Lykke.Service.Bil2IndexerWebApi.Extensions;
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
                BlockchainType = blockchainType,
                Links = new BlockchainLinksResponce
                {
                    AddressesUrl = Url.AddressesUrl(blockchainType, "_address_"),
                    AssetsUrl = Url.AssetsUrl(blockchainType),
                    BlocksUrl = Url.BlocksUrl(blockchainType),
                    TransactionsUrl = Url.TransactionsUrl(blockchainType, "_address_")
                }
            }).ToList());
        }
    }
}
