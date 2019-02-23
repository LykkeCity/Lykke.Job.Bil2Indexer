using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.Tests.Mocks
{
    internal class InMemoryBlocksRepository : IBlocksRepository
    {
        private readonly SortedList<long, BlockHeader> _blocks = new SortedList<long, BlockHeader>();

        public Task SaveAsync(BlockHeader block)
        {
            _blocks.Add(block.Number, block);

            Console.WriteLine($"Saved: {block}");

            return Task.CompletedTask;
        }

        public Task<BlockHeader> GetOrDefaultAsync(long blockNumber)
        {
            _blocks.TryGetValue(blockNumber, out var block);

            return Task.FromResult(block);
        }

        public Task RemoveAsync(BlockHeader block)
        {
            Console.WriteLine($"Removed: {block}");

            var storedBlock = _blocks[block.Number];

            if (storedBlock.Hash == block.Hash)
            {
                _blocks.Remove(storedBlock.Number);
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<BlockHeader>> GetAllAsync()
        {
            return Task.FromResult<IReadOnlyList<BlockHeader>>(new ReadOnlyCollection<BlockHeader>(_blocks.Values));
        }
    }
}
