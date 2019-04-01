using System;
using System.Collections.Generic;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
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
