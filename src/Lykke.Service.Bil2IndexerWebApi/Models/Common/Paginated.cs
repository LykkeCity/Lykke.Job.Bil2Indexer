using System.Collections.Generic;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Common
{
    public class Paginated<TItem, TId>
    {
        public PaginationModel<TId> Pagination { get; set; }
        public IReadOnlyCollection<TItem> Items { get; set; }
    }
}
