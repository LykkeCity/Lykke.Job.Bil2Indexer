using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Requests.Shared
{
    public class ByIdRequest
    {
        [FromRoute(Name = "blockchainType")]
        public string BlockchainType { get; set; }

        [FromRoute(Name = "id")]
        public string Id { get; set; }
    }
}
