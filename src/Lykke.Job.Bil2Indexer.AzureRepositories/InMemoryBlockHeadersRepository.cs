using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.AzureRepositories
{
    public class InMemoryBlockHeadersRepository : IBlockHeadersRepository
    {
        private readonly ConcurrentDictionary<(string, long), BlockHeader> _blocks;
        private readonly ILog _log;

        public InMemoryBlockHeadersRepository(ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);

            _blocks = new ConcurrentDictionary<(string, long), BlockHeader>();
        }

        public Task SaveAsync(BlockHeader block)
        {
            var key = (block.BlockchainType, block.Number);

            _blocks.AddOrUpdate(
                key,
                k =>
                {
                    _log.Info($"Block header saved {block}");

                    return block;
                },
                (k, oldBlock) =>
                {
                    if (oldBlock.Version != block.Version)
                    {
                        throw new InvalidOperationException(
                            $"Optimistic concurrency: block header version mismatch. Expected version {oldBlock.Version}, actual {block.Version}");
                    }

                    var newBlock = new BlockHeader
                    (
                        block.Id,
                        block.Version + 1,
                        block.BlockchainType,
                        block.Number,
                        block.MinedAt,
                        block.Size,
                        block.TransactionsCount,
                        block.PreviousBlockId,
                        block.State
                    );

                    _log.Info($"Block header saved {newBlock}");

                    return newBlock;
                });
            
            return Task.CompletedTask;
        }

        public Task<BlockHeader> GetOrDefaultAsync(string blockchainType, long blockNumber)
        {
            _blocks.TryGetValue((blockchainType, blockNumber), out var block);    

            return Task.FromResult(block);
        }

        public Task<BlockHeader> GetOrDefaultAsync(string blockchainType, string blockId)
        {
            var block = _blocks.Values.SingleOrDefault(x => x.BlockchainType == blockchainType && x.Id == blockId);

            return Task.FromResult(block);
        }

        public async Task<BlockHeader> GetAsync(string blockchainType, string blockId)
        {
            var block = await GetOrDefaultAsync(blockchainType, blockId);

            if (block == null)
            {
                throw new InvalidOperationException($"Block {blockchainType}:{blockId} is not found");
            }

            return block;
        }

        public Task RemoveIfExistAsync(string blockchainType, string blockId)
        {
            var block = _blocks.Values.SingleOrDefault(x => x.BlockchainType == blockchainType && x.Id == blockId);

            if(block != null)
            {
                _blocks.TryRemove((blockchainType, block.Number), out _);

                _log.Info($"Block header removed {block}");
            }

            return Task.CompletedTask;
        }
    }
}
