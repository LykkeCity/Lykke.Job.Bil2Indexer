using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.IndexerState;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.IndexerState.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.BlockHeaders
{
    public class BlockHeadersRepository:IBlockHeadersRepository
    {
        private readonly IPgConnectionStringProvider _connectionStringProvider;

        public BlockHeadersRepository(IPgConnectionStringProvider connectionStringProvider)
        {
            _connectionStringProvider = connectionStringProvider;
        }


        public async Task SaveAsync(BlockHeader block)
        {
            var dbEntity = Map(block);
            var isExisted = block.Version != 0;

            using (var db = new StateDataContext(_connectionStringProvider.GetConnectionString(block.BlockchainType)))
            {
                if (isExisted)
                {
                    db.BlockHeaders.Update(dbEntity);

                    try
                    {
                        await db.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException e)
                    {
                        throw new OptimisticConcurrencyException(e);
                    }
                }
                else
                {
                    await db.BlockHeaders.AddAsync(dbEntity);

                    try
                    {
                        await db.SaveChangesAsync();
                    }
                    catch (DbUpdateException e) when (e.IsNaturalKeyViolationException())
                    {
                        throw new OptimisticConcurrencyException(e);
                    }
                }
            }
        }

        public async Task<BlockHeader> GetOrDefaultAsync(string blockchainType, long blockNumber)
        {
            using (var db = new StateDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var existed = await db.BlockHeaders
                    .SingleOrDefaultAsync(BuildPredicate(blockNumber));

                return existed != null ? Map(existed, blockchainType) : null;
            }
        }

        public async Task<BlockHeader> GetOrDefaultAsync(string blockchainType, BlockId blockId)
        {
            using (var db = new StateDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var existed = await db.BlockHeaders
                    .SingleOrDefaultAsync(BuildPredicate(blockId));

                return existed != null ? Map(existed, blockchainType) : null;
            }
        }

        public async Task<BlockHeader> GetAsync(string blockchainType, long blockNumber)
        {
            var block = await GetOrDefaultAsync(blockchainType, blockNumber);

            if (block == null)
            {
                throw new InvalidOperationException($"Block {blockchainType}:{blockNumber} is not found");
            }

            return block;
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

        public async Task TryRemoveAsync(string blockchainType, BlockId blockId)
        {
            using (var db = new StateDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var existed = await db.BlockHeaders
                    .SingleOrDefaultAsync(BuildPredicate(blockId));

                if (existed != null)
                {
                    db.BlockHeaders.Remove(existed);

                    await db.SaveChangesAsync();
                }
            }
        }

        private Expression<Func<BlockHeaderEntity, bool>> BuildPredicate(BlockId blockId)
        {
            var stringBlockId = blockId.ToString();

            return p => p.Id == stringBlockId;

        }

        private Expression<Func<BlockHeaderEntity, bool>> BuildPredicate(long blockNumber)
        {
            return p => p.Number == blockNumber;

        }

        private BlockState Map(BlockHeaderEntity.BlockState source)
        {
            switch (source)
            {
                case BlockHeaderEntity.BlockState.Assembling:
                    return BlockState.Assembling;
                case BlockHeaderEntity.BlockState.Assembled:
                    return BlockState.Assembled;
                case BlockHeaderEntity.BlockState.Executed:
                    return BlockState.Executed;
                case BlockHeaderEntity.BlockState.PartiallyExecuted:
                    return BlockState.PartiallyExecuted;
                case BlockHeaderEntity.BlockState.RolledBack:
                    return BlockState.RolledBack;
                default:
                    throw new ArgumentException("Invalid switch", nameof(source));
            }
        }

        private BlockHeaderEntity.BlockState Map(BlockState source)
        {
            switch (source)
            {
                case BlockState.Assembling:
                    return BlockHeaderEntity.BlockState.Assembling;
                case BlockState.Assembled:
                    return BlockHeaderEntity.BlockState.Assembled;
                case BlockState.Executed:
                    return BlockHeaderEntity.BlockState.Executed;
                case BlockState.PartiallyExecuted:
                    return BlockHeaderEntity.BlockState.PartiallyExecuted;
                case BlockState.RolledBack:
                    return BlockHeaderEntity.BlockState.RolledBack;
                default:
                    throw new ArgumentException("Invalid switch", nameof(source));
            }
        }

        private BlockHeaderEntity Map(BlockHeader source)
        {
            return new BlockHeaderEntity
            {
                Version = (uint) source.Version,
                State = Map(source.State),
                Id = source.Id,
                MinedAt = source.MinedAt,
                Number = source.Number,
                PreviousBlockId = source.PreviousBlockId,
                Size = source.Size,
                TransactionCount = source.TransactionsCount
            };
        }

        private BlockHeader Map(BlockHeaderEntity source, string blockchainType)
        {
            return new BlockHeader(id: source.Id, 
                version: source.Version,
                blockchainType: blockchainType, 
                number: source.Number, 
                minedAt: source.MinedAt, 
                size: source.Size, 
                transactionsCount: source.TransactionCount, 
                previousBlockId: source.PreviousBlockId,
                state: Map(source.State));
        }
    }
}
