using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Common
{
    public class PaginationRequest<T>
    {
        [FromQuery]
        public PaginationOrder Order { get; set; }

        [FromQuery]
        public T StartingAfter { get; set; }

        [FromQuery]
        public T EndingBefore { get; set; }

        [FromQuery]
        public int Limit { get; set; } = 25;
    }
}
