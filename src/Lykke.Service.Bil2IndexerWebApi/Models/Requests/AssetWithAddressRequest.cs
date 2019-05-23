using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Requests
{
    public class AssetWithAddressRequest
    {
        [FromRoute(Name = "blockchainType")]
        public string BlockchainType { get; set; }

        [FromRoute(Name = "assetTicker")]
        public string AssetTicker { get; set; }

        [FromRoute(Name = "assetAddress")]
        public string AssetAddress { get; set; }
    }
}
