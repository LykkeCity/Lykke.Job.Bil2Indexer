using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Requests
{
    public class ByBlockHeightRequest
    {
        [FromRoute(Name = "blockchainType")]
        public string BlockchainType { get; set; }

        [FromRoute(Name = "height")]
        public long Height { get; set; }
    }
}
