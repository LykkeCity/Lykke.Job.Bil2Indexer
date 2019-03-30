using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.AzureRepositories
{
    public class InMemoryBlockHeadersRepository : IBlockHeadersRepository
    {
        private readonly SortedList<(string, long), BlockHeader> _blocks = new SortedList<(string, long), BlockHeader>();

        public Task SaveAsync(BlockHeader block)
        {
            lock (_blocks)
            {
                _blocks.TryAdd((block.BlockchainType, block.Number), block);

                Console.WriteLine($"Block header saved: {block}");
            }

            return Task.CompletedTask;
        }

        public Task<BlockHeader> GetOrDefaultAsync(string blockchainType, long blockNumber)
        {
            lock (_blocks)
            {
                _blocks.TryGetValue((blockchainType, blockNumber), out var block);    

                return Task.FromResult(block);
            }
        }

        public Task RemoveAsync(string blockchainType, string blockId)
        {
            lock (_blocks)
            {
                var block = _blocks.Values.SingleOrDefault(x => x.BlockchainType == blockchainType && x.Id == blockId);

                if(block != null)
                {
                    _blocks.Remove((blockchainType, block.Number));

                    Console.WriteLine($"Block header removed: {block}");
                }

                return Task.CompletedTask;
            }
        }

        public Task<BlockHeader> GetAsync(string blockchainType, string blockId)
        {
            lock (_blocks)
            {
                var block = _blocks.Values.SingleOrDefault(x => x.BlockchainType == blockchainType && x.Id == blockId);

                if (block == null)
                {
                    throw new InvalidOperationException($"Block {blockchainType}:{blockId} is not found");
                }

                return Task.FromResult(block);
            }
        }
    }
}
