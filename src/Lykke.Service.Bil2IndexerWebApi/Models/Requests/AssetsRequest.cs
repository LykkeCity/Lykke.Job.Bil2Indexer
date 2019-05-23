using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Requests
{
    public class AssetsRequest
    {
        public AssetsRequest()
        {
            Pagination = new PaginationRequest<string>();
        }

        [FromRoute(Name = "blockchainType")]
        public string BlockchainType { get; set; }

        [FromRoute(Name = "assetTicker")]
        public string AssetTicker { get; set; }

        [FromRoute(Name = "assetAddress")]
        public string AssetAddress { get; set; }

        [FromQuery]
        public PaginationRequest<string> Pagination { get; set; }
    }
}
