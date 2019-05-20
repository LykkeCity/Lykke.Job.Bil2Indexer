using System;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.IndexerState;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.IndexerState.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.ChainHeads
{
    public class ChainHeadsRepository:IChainHeadsRepository
    {
        private readonly IPgConnectionStringProvider _connectionStringProvider;

        public ChainHeadsRepository(IPgConnectionStringProvider connectionStringProvider)
        {
            _connectionStringProvider = connectionStringProvider;
        }

        public async Task<ChainHead> GetOrDefaultAsync(string blockchainType)
        {
            using (var db = new StateDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var existed = await db.ChainHeads.SingleOrDefaultAsync(p => p.Id == blockchainType);

                return existed != null ? Map(existed, blockchainType) : null;
            }
        }

        public async Task<ChainHead> GetAsync(string blockchainType)
        {
            using (var db = new StateDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var existed = await db.ChainHeads.SingleOrDefaultAsync(p => p.Id == blockchainType);

                if (existed == null)
                {
                    throw new InvalidOperationException($"ChainHead {blockchainType} is not found");
                }

                return Map(existed, blockchainType);
            }
        }

        public async Task SaveAsync(ChainHead head)
        {
            var dbEntity = Map(head);
            var isExisted = head.Version != 0;

            using (var db = new StateDataContext(_connectionStringProvider.GetConnectionString(head.BlockchainType)))
            {
                if (isExisted)
                {
                    db.ChainHeads.Update(dbEntity);

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
                    await db.ChainHeads.AddAsync(dbEntity);

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

        private static ChainHead Map(ChainHeadEntity source, string blockchainType)
        {
            return new ChainHead
            (
                blockchainType,
                source.FirstBlockNumber,
                source.Version,
                source.Sequence,
                source.CrawlerSequence,
                source.BlockNumber,
                source.BlockId,
                source.PreviousBlockId,
                source.Mode
            );
        }

        private static ChainHeadEntity Map(ChainHead source)
        {
            return new ChainHeadEntity
            {
                Version = (uint) source.Version,
                Sequence = source.Sequence,
                CrawlerSequence = source.CrawlerSequence,
                Id = source.BlockchainType,
                FirstBlockNumber = source.FirstBlockNumber,
                BlockId = source.BlockId,
                PreviousBlockId = source.PreviousBlockId,
                BlockNumber = source.BlockNumber,
                Mode = source.Mode
            };
        }
    }
}
