using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Requests
{
    public class BlocksRequest: PaginationRequest<long?>
    {
        [FromRoute(Name = "blockchainType")]
        public string BlockchainType { get; set; }
    }
}
