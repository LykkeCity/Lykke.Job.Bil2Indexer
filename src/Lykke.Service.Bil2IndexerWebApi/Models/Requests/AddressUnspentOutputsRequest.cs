using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Requests
{
    public class AddressUnspentOutputsRequest
    {
        public AddressUnspentOutputsRequest()
        {
            Pagination = new PaginationRequest<string>();
        }

        [FromQuery(Name = "")]
        public PaginationRequest<string> Pagination { get; set; }

        [FromRoute(Name = "blockchainType")]
        public string BlockchainType { get; set; }

        [FromRoute(Name = "address")]
        public string Address { get; set; }
    }
}
