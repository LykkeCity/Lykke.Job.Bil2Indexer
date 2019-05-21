using System;
using System.Threading.Tasks;
using Lykke.Service.Bil2IndexerWebApi.Mappers;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;
using Lykke.Service.Bil2IndexerWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Controllers
{
    [Route("api/blockchains/{blockchainType}/assets")]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetQueryFacade _assetQueryFacade;

        public AssetsController(IAssetQueryFacade assetQueryFacade)
        {
            _assetQueryFacade = assetQueryFacade;
        }

        [HttpGet(Name = nameof(GetAssets))]
        public async Task<ActionResult<Paginated<AssetModel, string>>> GetAssets(
            [FromRoute] string blockchainType,
            [FromQuery] string assetTicker,
            [FromQuery] string assetAddress,
            PaginationRequest<string> pagination)
        {
            Paginated<AssetModel, string> result;

            if (assetTicker != null && assetAddress != null)
            {
                var asset = await _assetQueryFacade.GetAsset(blockchainType, assetAddress, assetTicker);

                result = asset.PaginateSingle(pagination);
            }
            else
            {
                result = (await _assetQueryFacade.GetAssets(blockchainType, 
                    pagination.Limit, 
                    pagination.Order == PaginationOrder.Asc, 
                    pagination.StartingAfter,
                    pagination.EndingBefore)).Paginate(pagination);
            }

            return Ok(result);
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
