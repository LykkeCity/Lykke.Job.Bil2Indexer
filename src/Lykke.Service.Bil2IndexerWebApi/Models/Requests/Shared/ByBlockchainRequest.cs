using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Requests.Shared
{
    public class ByBlockchainRequest
    {
        [FromRoute]
        public string BlockchainType { get; set; }
    }
}
