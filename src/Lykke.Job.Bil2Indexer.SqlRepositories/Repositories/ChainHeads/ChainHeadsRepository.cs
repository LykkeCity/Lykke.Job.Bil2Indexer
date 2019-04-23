using System;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.IndexerState;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.IndexerState.Models;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.ChainHeads
{
    public class ChainHeadsRepository:IChainHeadsRepository
    {
        private readonly string _postgresConnString;

        public ChainHeadsRepository(string postgresConnString)
        {
            _postgresConnString = postgresConnString;
        }

        public async Task<ChainHead> GetOrDefaultAsync(string blockchainType)
        {
            using (var db = new StateDataContext(_postgresConnString))
            {
                var existed = await db.ChainHeads.SingleOrDefaultAsync(p => p.BlockchainType == blockchainType);

                return existed != null ? Map(existed) : null;
            }
        }

        public async Task<ChainHead> GetAsync(string blockchainType)
        {
            using (var db = new StateDataContext(_postgresConnString))
            {
                var existed = await db.ChainHeads.SingleOrDefaultAsync(p => p.BlockchainType == blockchainType);

                if (existed == null)
                {
                    throw new InvalidOperationException($"ChainHead {blockchainType} is not found");
                }

                return Map(existed);
            }
        }

        public async Task SaveAsync(ChainHead head)
        {
            var dbEntity = Map(head);
            var isExisted = head.Version != 0;

            using (var db = new StateDataContext(_postgresConnString))
            {
                if (isExisted)
                {
                    db.ChainHeads.Update(dbEntity);
                }
                else
                {
                    await db.ChainHeads.AddAsync(dbEntity);
                }
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

        private static ChainHead Map(ChainHeadEntity source)
        {
            return new ChainHead
            (
                source.BlockchainType,
                source.FirstBlockNumber,
                source.Version,
                source.BlockNumber,
                source.BlockId,
                source.PreviousBlockId
            );
        }

        private static ChainHeadEntity Map(ChainHead source)
        {
            return new ChainHeadEntity
            {
                Version = (uint) source.Version,
                BlockchainType = source.BlockchainType,
                FirstBlockNumber = source.FirstBlockNumber,
                BlockId = source.BlockId,
                PreviousBlockId = source.PreviousBlockId,
                BlockNumber = source.BlockNumber
            };
        }
    }
}
