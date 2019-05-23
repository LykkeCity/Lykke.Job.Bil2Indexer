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

        [FromQuery(Name = "assetTicker")]
        public string AssetTicker { get; set; }

        [FromQuery(Name = "assetAddress")]
        public string AssetAddress { get; set; }

        [FromQuery(Name = "")]
        public PaginationRequest<string> Pagination { get; set; }
    }
}
