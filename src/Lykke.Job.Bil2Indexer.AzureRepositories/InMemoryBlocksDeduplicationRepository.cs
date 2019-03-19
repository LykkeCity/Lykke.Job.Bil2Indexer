using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.AzureRepositories
{
    public class InMemoryBlocksDeduplicationRepository : IBlocksDeduplicationRepository
    {
        private readonly ConcurrentDictionary<string, bool> _blockIds;

        public InMemoryBlocksDeduplicationRepository()
        {
            _blockIds = new ConcurrentDictionary<string, bool>();
        }

        public Task<bool> IsProcessedAsync(string blockId)
        {
            return Task.FromResult(_blockIds.ContainsKey(blockId));
        }

        public Task MarkAsProcessedAsync(string blockId)
        {
            _blockIds.TryAdd(blockId, false);

            return Task.CompletedTask;
        }

        public Task MarkAsNotProcessedAsync(string blockId)
        {
            _blockIds.Remove(blockId, out _);

            return Task.CompletedTask;
        }
    }
}
