using System;
using System.Threading.Tasks;
using DataApi.Core.Domain;
using DataApi.Factories;
using DataApi.Models;
using DataApi.Models.Common;
using DataApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("api/[controller]")]
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
        public async Task<ActionResult<Paginated<AssetModel[]>>> GetAssets([FromRoute] string address,
            [FromRoute] string ticker,
            PaginationOrder order, string startingAfter, string endingBefore, int limit = 25)
        {
            Asset[] assets = null;

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
    }
}