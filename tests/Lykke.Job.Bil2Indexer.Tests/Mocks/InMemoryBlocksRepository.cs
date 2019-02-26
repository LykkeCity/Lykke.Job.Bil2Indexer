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
            lock (_blocks)
            {
                _blocks.Add(block.Number, block);

                Console.WriteLine($"Saved: {block}");
            }

            return Task.CompletedTask;
        }

        public Task<BlockHeader> GetOrDefaultAsync(long blockNumber)
        {
            lock (_blocks)
            {
                _blocks.TryGetValue(blockNumber, out var block);    

                return Task.FromResult(block);
            }
        }

        public Task RemoveAsync(BlockHeader block)
        {
            lock (_blocks)
            {
                var storedBlock = _blocks[block.Number];

                if (storedBlock.Hash == block.Hash)
                {
                    _blocks.Remove(storedBlock.Number);
                }

                Console.WriteLine($"Removed: {block}");

                return Task.CompletedTask;
            }
        }

        public Task<IReadOnlyList<BlockHeader>> GetAllAsync()
        {
            lock (_blocks)
            {
                return Task.FromResult<IReadOnlyList<BlockHeader>>(new ReadOnlyCollection<BlockHeader>(_blocks.Values));
            }
        }
    }
}
