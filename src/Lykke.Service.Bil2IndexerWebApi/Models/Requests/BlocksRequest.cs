using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Requests
{
    public class BlocksRequest
    {
        public BlocksRequest()
        {
            Pagination = new PaginationRequest<long>();
        }

        [FromRoute(Name = "blockchainType")]
        public string BlockchainType { get; set; }

        [FromQuery(Name = "")]
        public PaginationRequest<long> Pagination { get; set; }
    }
}
