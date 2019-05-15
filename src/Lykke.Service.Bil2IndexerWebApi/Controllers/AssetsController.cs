using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Mappers;
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
        private readonly IAssetQueryFacade _assetQueryFacade;

        public AssetsController(IAssetQueryFacade assetQueryFacade)
        {
            _assetQueryFacade = assetQueryFacade;
        }

        [HttpGet(Name = nameof(GetAssets))]
        public async Task<ActionResult<Paginated<AssetModel>>> GetAssets(
            [FromRoute] string blockchainType,
            [FromQuery] string assetTicker,
            [FromQuery] string assetAddress,
            [FromQuery] PaginationOrder order, 
            [FromQuery] string startingAfter, 
            [FromQuery] string endingBefore, 
            [FromQuery] int limit = 25)
        {
            IReadOnlyCollection<AssetInfo> assets;

            if (assetTicker != null && assetAddress != null)
            {
                var asset = await _assetQueryFacade.GetAsset(blockchainType, assetAddress, assetTicker);

                assets = new []{asset};
            }
            else
            {
                assets = await _assetQueryFacade.GetAssets(blockchainType, limit, order == PaginationOrder.Asc, startingAfter, endingBefore);
            }

            var model = AssetModelMapper.ToViewModel(assets);

            return model;
        }

        [HttpGet("/{assetTicker}/without-address", Name = nameof(GetAssetWithoutAddress))]
        public async Task<ActionResult<AssetModel>> GetAssetWithoutAddress(
            [FromRoute] string blockchainType,
            [FromRoute] string assetTicker)
        {
            throw new NotImplementedException();
        }

        [HttpGet("/{assetTicker}/addresses/{assetAddress}", Name = nameof(GetAssetWithAddress))]
        public async Task<ActionResult<AssetModel>> GetAssetWithAddress(
            [FromRoute] string blockchainType,
            [FromRoute] string assetTicker,
            [FromRoute] string assetAddress)
        {
            throw new NotImplementedException();
        }
    }
}
