using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Factories;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;
using Lykke.Service.Bil2IndexerWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Controllers
{
    [Route("api/blockchains/{blockchainType}/assets")]
    [ApiController]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetService _assetService;
        private readonly IAssetModelFactory _assetModelFactory;

        public AssetsController(IAssetService assetService, IAssetModelFactory assetModelFactory)
        {
            _assetService = assetService;
            _assetModelFactory = assetModelFactory;
        }

        [HttpGet(Name = nameof(GetAssets))]
        public async Task<ActionResult<Paginated<AssetModel[]>>> GetAssets(
            [FromRoute] string blockchainType,
            [FromQuery] string assetTicker,
            [FromQuery] string assetAddress,
            PaginationOrder order, 
            string startingAfter, 
            string endingBefore, 
            int limit = 25)
        {
            AssetInfo[] assets = null;

            if (address != null && ticker != null)
            {
                var asset = await _assetService.GetAsset(address, ticker);
                assets = new []{asset};
            }
            else
            {
                assets = await _assetService.GetAssets(limit, order == PaginationOrder.Asc,
                    startingAfter, endingBefore);
            }

            var model = _assetModelFactory.PrepareAssetsPaginated(assets);

            return model;
        }

        [HttpGet("/{assetTicker}", Name = nameof(GetAssets))]
        public async Task<ActionResult<Paginated<AssetModel[]>>> GetAssets(
            [FromRoute] string blockchainType,
            [FromRoute] string assetTicker)
        {
            AssetInfo asset = null;


            var model = _assetModelFactory.PrepareAssetsPaginated(assets);

            return model;
        }

        [HttpGet("/{assetTicker}/addresses/{assetAddress}", Name = nameof(GetAssets))]
        public async Task<ActionResult<Paginated<AssetModel[]>>> GetAssets(
            [FromRoute] string blockchainType,
            [FromRoute] string assetTicker,
            [FromRoute] string assetAddress)
        {
            AssetInfo asset = null;


            var model = _assetModelFactory.PrepareAssetsPaginated(assets);

            return model;
        }
    }
}
