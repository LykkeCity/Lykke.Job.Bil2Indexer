using System;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public class OptimisticConcurrencyException:Exception
    {
        public OptimisticConcurrencyException()
        {

        }

        public OptimisticConcurrencyException(Exception inner) : base("Optimistic concurrency error", inner)
        {

        }
    }
}
