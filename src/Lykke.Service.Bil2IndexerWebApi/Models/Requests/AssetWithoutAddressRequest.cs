using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Requests
{
    public class AssetWithoutAddressRequest
    {
        [FromRoute(Name = "blockchainType")]
        public string BlockchainType { get; set; }

        [FromRoute(Name = "assetTicker")]
        public string AssetTicker { get; set; }
    }
}
