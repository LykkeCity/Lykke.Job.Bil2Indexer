using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Common
{
    public class PaginationRequest<T>
    {
        
        public PaginationOrder Order { get; set; }

        public T StartingAfter { get; set; }

        public T EndingBefore { get; set; }

        public int Limit { get; set; } = 25;
    }
}
