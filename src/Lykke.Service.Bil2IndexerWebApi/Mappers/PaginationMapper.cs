using System.Collections.Generic;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class PaginationMapper
    {
        //TODO pass build url func
        public static Paginated<T> Paginate<T>(this IReadOnlyCollection<T> source, PaginationRequest pagination)
        {
            return new Paginated<T>()
            {
                Items = source,
                Pagination = MapPaginationModel(pagination, source.Count)
            };
        }

        //TODO pass build url func
        public static Paginated<T> PaginateSingle<T>(this T source, PaginationRequest pagination)
        {
            var items = source != null ? new[] { source } : new T[0];

            return items.Paginate(pagination);
        }

        private static PaginationModel MapPaginationModel(PaginationRequest source, int count)
        {
            return new PaginationModel
            {
                Count = source.Limit,
                Order = source.Order,
                EndingBefore = source.EndingBefore,
                NextUrl = "TODO",
                PrevUrl = "TODO",
                StartingAfter = source.StartingAfter
            };
        }
    }
}
