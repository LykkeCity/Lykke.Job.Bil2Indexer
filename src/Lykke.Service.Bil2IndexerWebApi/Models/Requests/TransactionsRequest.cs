using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Requests
{
    public class TransactionsRequest
    {
        [FromRoute(Name = "blockchainType")]
        public string BlockchainType { get; set; }

        [FromQuery(Name = "blockId")]
        public string BlockId { get; set; }

        [FromQuery(Name = "blockNumber")]
        public int? BlockNumber { get; set; }

        [FromQuery(Name = "address")]
        public string Address { get; set; }

        [FromQuery]
        public PaginationRequest<string> Pagination { get; set; }
    }
}
