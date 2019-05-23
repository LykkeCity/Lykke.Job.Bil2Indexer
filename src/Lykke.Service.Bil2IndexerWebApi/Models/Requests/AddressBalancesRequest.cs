using System;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Requests
{
    public class AddressBalancesRequest
    {
        [FromRoute(Name = "address")]
        public string Address { get; set; }

        [FromRoute(Name = "blockchainType")]
        public string BlockchainType { get; set; }

        [FromQuery(Name = "blockId")]
        public string BlockId { get; set; }

        [FromQuery(Name = "blockNumber")]
        public long? BlockNumber { get; set; }

        [FromQuery(Name = "dateTime")]
        public DateTimeOffset? DateTime { get; set; }
    }
}
