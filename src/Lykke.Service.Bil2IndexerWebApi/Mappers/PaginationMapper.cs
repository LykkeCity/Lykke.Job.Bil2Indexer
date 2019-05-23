using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class PaginationMapper
    {
        //rewrite ugly way
        public static Paginated<TItem, TId> Paginate<TItem, TId>(this IReadOnlyCollection<TItem> source, 
            PaginationRequest<TId> request, 
            IUrlHelper url,
            Func<TItem, TId> idProjection)
        {

            return new Paginated<TItem, TId>
            {
                Items = source,
                Pagination = MapPaginationModel(request, 
                    url, 
                    source, 
                    idProjection)
            };
        }

        private static PaginationModel<TId> MapPaginationModel<TId, TItem>(PaginationRequest<TId> request,
            IUrlHelper url,
            IReadOnlyCollection<TItem> items,
            Func<TItem, TId> idProjection)
        {
            var result = new PaginationModel<TId>
            {
                Count = items.Count,
                Order = request.Order,
                EndingBefore = request.EndingBefore,
                StartingAfter = request.StartingAfter,
            };

            if (items.Any() && result.StartingAfter != null)
            {
                request.EndingBefore = idProjection(items.First());
                request.StartingAfter = default;
                result.PrevUrl = BuildUrl(url, request);
            }

            if (items.Any() && items.Count == request.Limit)
            {
                request.StartingAfter = idProjection(items.Last());
                request.EndingBefore = default;
                result.NextUrl = BuildUrl(url, request);
            }

            return result;
        }

        private static string BuildUrl<T>(IUrlHelper url, PaginationRequest<T> request)
        {
            var controller = url.ActionContext.RouteData.Values["controller"].ToString();
            var action = url.ActionContext.RouteData.Values["action"].ToString();

            return url.Action(action, controller, request);
        }
    }
}
