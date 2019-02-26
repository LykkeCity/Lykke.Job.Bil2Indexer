using System;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.Tests.Mocks
{
    internal class InMemoryBlockExpectationRepository : IBlockExpectationRepository
    {
        private BlockExpectation _storedExpectation;
        private readonly object _lock = new object();

        public Task<BlockExpectation> GetOrDefaultAsync()
        {
            lock (_lock)
            {
                return _storedExpectation != null
                    ? Task.FromResult(_storedExpectation)
                    : Task.FromResult<BlockExpectation>(null);
            }
        }

        public Task SaveAsync(BlockExpectation blockExpectation)
        {
            lock (_lock)
            {
                if (_storedExpectation?.Version != blockExpectation.Version)
                {
                    throw new InvalidOperationException($"Optimistic concurrency: expected block was modified by someone else. Passed block [{blockExpectation}] does not match stored block [{_storedExpectation}]");
                }

                _storedExpectation = new BlockExpectation(Guid.NewGuid().ToString("N"), blockExpectation.Number);
            }

            return Task.CompletedTask;
        }
    }
}