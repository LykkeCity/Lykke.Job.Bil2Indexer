using System.Collections.Generic;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class PaginationMapper
    {
        //TODO pass build url func
        public static Paginated<TItem, TId> Paginate<TItem, TId>(this IReadOnlyCollection<TItem> source, PaginationRequest<TId> pagination)
        {
            return new Paginated<TItem, TId>()
            {
                Items = source,
                Pagination = MapPaginationModel(pagination, source.Count)
            };
        }

        //TODO pass build url func
        public static Paginated<TItem, TId> PaginateSingle<TItem, TId>(this TItem source, PaginationRequest<TId> pagination)
        {
            var items = source != null ? new[] { source } : new TItem[0];

            return items.Paginate(pagination);
        }

        private static PaginationModel<T> MapPaginationModel<T>(PaginationRequest<T> source, int count)
        {
            return new PaginationModel<T>
            {
                Count = count,
                Order = source.Order,
                EndingBefore = source.EndingBefore,
                StartingAfter = source.StartingAfter,
                //TODO
                NextUrl = null,
                //TODO
                PrevUrl = null
            };
        }
    }
}
