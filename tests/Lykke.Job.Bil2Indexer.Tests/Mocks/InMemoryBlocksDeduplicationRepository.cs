using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.Tests.Mocks
{
    internal class InMemoryBlocksDeduplicationRepository : IBlocksDeduplicationRepository
    {
        private readonly ConcurrentDictionary<string, bool> _blockHashes;

        public InMemoryBlocksDeduplicationRepository()
        {
            _blockHashes = new ConcurrentDictionary<string, bool>();
        }

        public Task<bool> IsProcessedAsync(string blockHash)
        {
            return Task.FromResult(_blockHashes.ContainsKey(blockHash));
        }

        public Task MarkAsProcessedAsync(string blockHash)
        {
            _blockHashes.TryAdd(blockHash, false);

            return Task.CompletedTask;
        }

        public Task MarkAsNotProcessedAsync(string blockHash)
        {
            _blockHashes.Remove(blockHash, out _);

            return Task.CompletedTask;
        }
    }
}
