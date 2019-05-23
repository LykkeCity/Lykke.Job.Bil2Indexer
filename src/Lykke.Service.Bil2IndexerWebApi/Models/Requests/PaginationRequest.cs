using Lykke.Service.Bil2IndexerWebApi.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Requests
{
    public class PaginationRequest<T>
    {
        
        [FromQuery(Name = "order")]
        public PaginationOrder Order { get; set; }

        [FromQuery(Name = "startingAfter")]
        public T StartingAfter { get; set; }

        [FromQuery(Name = "endingBefore")]
        public T EndingBefore { get; set; }

        [FromQuery(Name = "limit")]
        public int Limit { get; set; } = 25;
    }
}
