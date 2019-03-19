using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.AzureRepositories
{
    public class InMemoryBlockExpectationRepository : IBlockExpectationRepository
    {
        private readonly ConcurrentDictionary<string, BlockExpectation> _storedExpectations;

        public InMemoryBlockExpectationRepository()
        {
            _storedExpectations = new ConcurrentDictionary<string, BlockExpectation>();
        }

        public Task<BlockExpectation> GetOrDefaultAsync(string crawlerId)
        {
            _storedExpectations.TryGetValue(crawlerId, out var expectation);

            return Task.FromResult(expectation);
        }

        public Task SaveAsync(string crawlerId, BlockExpectation blockExpectation)
        {
            _storedExpectations.AddOrUpdate
            (
                crawlerId,
                id => blockExpectation,
                (id, storedExpectation) =>
                {
                    if (storedExpectation?.Version != blockExpectation.Version)
                    {
                        throw new InvalidOperationException($"Optimistic concurrency: expected block was modified by someone else. Passed block [{blockExpectation}] does not match stored block [{_storedExpectations}]");
                    }

                    return blockExpectation;
                }
            );

            Console.WriteLine($"Expected saved: {crawlerId}: {blockExpectation.Number}");

            return Task.CompletedTask;
        }
    }
}
