using System.Threading.Tasks;
using Lykke.Service.Bil2IndexerWebApi.Mappers;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests;
using Lykke.Service.Bil2IndexerWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Controllers
{
    public class AssetsController : ControllerBase
    {
        private readonly IAssetQueryFacade _assetQueryFacade;
        private const string RoutePrefix = "api/blockchains/{blockchainType}/assets";

        public AssetsController(IAssetQueryFacade assetQueryFacade)
        {
            _assetQueryFacade = assetQueryFacade;
        }

        [HttpGet(RoutePrefix, Name = nameof(GetAssets))]
        public async Task<ActionResult<Paginated<AssetModel, string>>> GetAssets(
            [FromQuery][FromRoute] AssetsRequest request)
        {
            Paginated<AssetModel, string> result;

            if (request.AssetTicker != null && request.AssetAddress != null)
            {
                var asset = await _assetQueryFacade.GetAsset(request.BlockchainType, request.AssetAddress, request.AssetTicker);

                result = asset.PaginateSingle(request.Pagination);
            }
            else
            {
                result = (await _assetQueryFacade.GetAssets(request.BlockchainType,
                    request.Pagination.Limit,
                    request.Pagination.Order == PaginationOrder.Asc,
                    request.Pagination.StartingAfter,
                    request.Pagination.EndingBefore)).Paginate(request.Pagination);
            }

            return result;
        }

        [HttpGet(RoutePrefix + "/{assetTicker}/without-address", Name = nameof(GetAssetWithoutAddress))]
        public async Task<ActionResult<AssetModel>> GetAssetWithoutAddress(
            [FromRoute] string blockchainType,
            [FromRoute] string assetTicker)
        {
            return await _assetQueryFacade.GetAsset(blockchainType, assetTicker);
        }

        [HttpGet(RoutePrefix + "/{assetTicker}/addresses/{assetAddress}", Name = nameof(GetAssetWithAddress))]
        public async Task<ActionResult<AssetModel>> GetAssetWithAddress(
            [FromRoute] string blockchainType,
            [FromRoute] string assetTicker,
            [FromRoute] string assetAddress)
        {
            return await _assetQueryFacade.GetAsset(blockchainType, assetTicker, assetAddress);
        }
    }
}
