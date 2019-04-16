using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.SharedDomain;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.AzureRepositories
{
    public class InMemoryBlockHeadersRepository : IBlockHeadersRepository
    {
        private readonly ConcurrentDictionary<(string, BlockId), BlockHeader> _blocks;
        private readonly ILog _log;

        public InMemoryBlockHeadersRepository(ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);

            _blocks = new ConcurrentDictionary<(string, BlockId), BlockHeader>();
        }

        public Task SaveAsync(BlockHeader block)
        {
            var key = (block.BlockchainType, block.Id);

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
            var block = _blocks.Values.SingleOrDefault(x => x.BlockchainType == blockchainType && x.Number == blockNumber);

            return Task.FromResult(block);
        }

        public Task<BlockHeader> GetOrDefaultAsync(string blockchainType, BlockId blockId)
        {
            _blocks.TryGetValue((blockchainType, blockId), out var block);    

            return Task.FromResult(block);
        }

        public async Task<BlockHeader> GetAsync(string blockchainType, BlockId blockId)
        {
            var block = await GetOrDefaultAsync(blockchainType, blockId);

            if (block == null)
            {
                throw new InvalidOperationException($"Block {blockchainType}:{blockId} is not found");
            }

            return block;
        }

        public Task TryRemoveAsync(string blockchainType, BlockId blockId)
        {
            if (_blocks.TryRemove((blockchainType, blockId), out var block))
            {
                _log.Info($"Block header removed {block}");
            }

            return Task.CompletedTask;
        }
    }
}
