using System;
using System.Collections.Generic;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public static class PaginatedItems
    {
        public static PaginatedItems<TItem> From<TItem>(string continuation, IReadOnlyCollection<TItem> items)
        {
            return new PaginatedItems<TItem>(continuation, items);
        }
    }

    public class PaginatedItems<TItem>
    {
        public static readonly PaginatedItems<TItem> Empty = new PaginatedItems<TItem>(null, Array.Empty<TItem>());

        public IReadOnlyCollection<TItem> Items { get; }

        public string Continuation { get; }

        public PaginatedItems(string continuation, IReadOnlyCollection<TItem> items)
        {
            Continuation = continuation;
            Items = items;
        }
    }
}
