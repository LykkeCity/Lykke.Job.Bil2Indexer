﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.IndexerState;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.BlockHeaders.Mappers;
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
            var dbEntity = block.ToDbEntity();
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
                    .SingleOrDefaultAsync(BlockHeadersPredicates.Build(blockNumber));

                return existed?.ToDomain(blockchainType);
            }
        }

        public async Task<BlockHeader> GetOrDefaultAsync(string blockchainType, BlockId blockId)
        {
            using (var db = new StateDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var existed = await db.BlockHeaders
                    .SingleOrDefaultAsync(BlockHeadersPredicates.Build(blockId));

                return existed?.ToDomain(blockchainType);
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

        public async Task<BlockHeader> GetAsync(string blockchainType, DateTime dateTime)
        {
            using (var db = new StateDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var existed = await db.BlockHeaders
                    .FirstOrDefaultAsync(BlockHeadersPredicates.Build(dateTime));

                return existed?.ToDomain(blockchainType);
            }
        }

        public async Task<IReadOnlyCollection<BlockHeader>> GetCollectionAsync(string blockchainType,  long maxBlockNumber, int limit, bool orderAsc, long? startingAfterNumber = null,
            long? endingBeforeNumber = null)
        {
            using (var db = new StateDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var query = db.BlockHeaders
                    .Where(BlockHeadersPredicates.BuildEnumeration(maxBlockNumber, startingAfterNumber, endingBeforeNumber, orderAsc));

                if (orderAsc)
                {
                    query = query.OrderBy(p => p.Number);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Number);
                }

                var entities = await query
                    .Take(limit)
                    .ToListAsync();

                return entities.Select(p => p.ToDomain(blockchainType)).ToList();
            }
        }

        public async Task TryRemoveAsync(string blockchainType, BlockId blockId)
        {
            using (var db = new StateDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var existed = await db.BlockHeaders
                    .SingleOrDefaultAsync(BlockHeadersPredicates.Build(blockId));

                if (existed != null)
                {
                    db.BlockHeaders.Remove(existed);

                    await db.SaveChangesAsync();
                }
            }
        }

    }
}
