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
        private readonly string _posgresConnString;

        public BlockHeadersRepository(string posgresConnString)
        {
            _posgresConnString = posgresConnString;
        }

        public async Task SaveAsync(BlockHeader block)
        {
            using (var db = new StateDataContext(_posgresConnString))
            {
                var newValues = Map(block);

                await db.BlockHeaders.AddAsync(newValues);

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException dbUpdEx) when (dbUpdEx.IsUniqueConstraintViolationException())
                {
                    var existed = await db.BlockHeaders
                        .SingleOrDefaultAsync(BuildPredicate(block.BlockchainType, block.Id));

                    if (existed == null)
                    {
                        throw;
                    }

                    db.Entry(newValues).State = EntityState.Detached;

                    db.Entry(existed).Property(nameof(BlockHeaderEntity.Version)).OriginalValue = newValues.Version;
                    db.Entry(existed).State = EntityState.Modified; //forces to update xmin even if actual prop is the same

                    db.Entry(existed).CurrentValues.SetValues(newValues);

                    try
                    {
                        await db.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException e)
                    {
                        throw new OptimisticConcurrencyException(e);
                    }
                }
            }
        }

        public async Task<BlockHeader> GetOrDefaultAsync(string blockchainType, long blockNumber)
        {
            using (var db = new StateDataContext(_posgresConnString))
            {
                var existed = await db.BlockHeaders
                    .SingleOrDefaultAsync(p => p.BlockchainType == blockchainType && p.Number == blockNumber);

                return existed != null ? Map(existed) : null;
            }
        }

        public async Task<BlockHeader> GetOrDefaultAsync(string blockchainType, BlockId blockId)
        {
            using (var db = new StateDataContext(_posgresConnString))
            {
                var existed = await db.BlockHeaders
                    .SingleOrDefaultAsync(BuildPredicate(blockchainType, blockId));

                return existed != null ? Map(existed) : null;
            }
        }

        public async Task<BlockHeader> GetAsync(string blockchainType, BlockId blockId)
        {
            using (var db = new StateDataContext(_posgresConnString))
            {
                var existed = await db.BlockHeaders
                    .SingleOrDefaultAsync(BuildPredicate(blockchainType, blockId));

                if (existed == null)
                {
                    throw new InvalidOperationException($"Block {blockchainType}:{blockId} is not found");
                }

                return Map(existed);
            }
        }

        public async Task TryRemoveAsync(string blockchainType, BlockId blockId)
        {
            using (var db = new StateDataContext(_posgresConnString))
            {
                var existed = await db.BlockHeaders
                    .SingleOrDefaultAsync(BuildPredicate(blockchainType, blockId));

                if (existed != null)
                {
                    db.BlockHeaders.Remove(existed);

                    await db.SaveChangesAsync();
                }
            }
        }

        private Expression<Func<BlockHeaderEntity, bool>> BuildPredicate(string blockchainType, BlockId blockId)
        {
            var stringBlockId = blockId.ToString();

            return p => p.BlockchainType == blockchainType && p.Id == stringBlockId;

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
                BlockchainType = source.BlockchainType,
                State = Map(source.State),
                Id = source.Id,
                MinedAt = source.MinedAt,
                Number = source.Number,
                PreviousBlockId = source.PreviousBlockId,
                Size = source.Size,
                TransactionCount = source.TransactionsCount
            };
        }

        private BlockHeader Map(BlockHeaderEntity source)
        {
            return new BlockHeader(id: source.Id, 
                version: source.Version,
                blockchainType:source.BlockchainType, 
                number: source.Number, 
                minedAt: source.MinedAt, 
                size: source.Size, 
                transactionsCount: source.TransactionCount, 
                previousBlockId: source.PreviousBlockId,
                state: Map(source.State));
        }
    }
}
