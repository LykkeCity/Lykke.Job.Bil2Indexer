using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Requests
{
    public class AddressUnspentOutputsRequest: PaginationRequest<string>
    {
        [FromRoute(Name = "blockchainType")]
        public string BlockchainType { get; set; }

        [FromRoute(Name = "address")]
        public string Address { get; set; }
    }
}
