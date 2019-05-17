using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Common
{
    public class PaginationRequest
    {
        [FromQuery]
        public PaginationOrder Order { get; set; }

        [FromQuery]
        public string StartingAfter { get; set; }

        [FromQuery]
        public string EndingBefore { get; set; }

        [FromQuery]
        public int Limit { get; set; } = 25;
    }
}
