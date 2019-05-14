using System.Collections.Generic;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Common
{
    public class Paginated<T>
    {
        public PaginationModel Pagination { get; set; }
        public IReadOnlyCollection<T> Items { get; set; }
    }
}
