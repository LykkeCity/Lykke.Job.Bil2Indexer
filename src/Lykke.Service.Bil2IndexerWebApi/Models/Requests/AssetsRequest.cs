using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Requests
{
    public class AssetsRequest: PaginationRequest<string>
    {
        [FromRoute(Name = "blockchainType")]
        public string BlockchainType { get; set; }
    }
}
