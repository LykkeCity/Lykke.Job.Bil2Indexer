﻿using System;
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
        private readonly string _posgresConnString;

        public ChainHeadsRepository(string posgresConnString)
        {
            _posgresConnString = posgresConnString;
        }

        public async Task<ChainHead> GetOrDefaultAsync(string blockchainType)
        {
            using (var db = new StateDataContext(_posgresConnString))
            {
                var existed = await db.ChainHeads.SingleOrDefaultAsync(p => p.BlockchainType == blockchainType);

                return existed != null ? Map(existed) : null;
            }
        }

        public async Task<ChainHead> GetAsync(string blockchainType)
        {
            using (var db = new StateDataContext(_posgresConnString))
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
            using (var db = new StateDataContext(_posgresConnString))
            {
                var newValues = Map(head);
                
                await db.ChainHeads.AddAsync(newValues);

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException dbUpdEx) when (dbUpdEx.IsConstraintViolationException())
                {
                    var existed = await db.ChainHeads
                        .SingleOrDefaultAsync(p => p.BlockchainType == head.BlockchainType);

                    if (existed == null)
                    {
                        throw;
                    }

                    db.Entry(newValues).State = EntityState.Detached;

                    db.Entry(existed).Property(nameof(ChainHeadEntity.Version)).OriginalValue = newValues.Version;
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

        private ChainHead Map(ChainHeadEntity source)
        {
            return new ChainHead(source.BlockchainType, 
                source.FirstBlockNumber, 
                source.Version, 
                source.BlockNumber,
                source.BlockId);
        }

        private ChainHeadEntity Map(ChainHead source)
        {
            return new ChainHeadEntity
            {
                Version = (uint) source.Version,
                BlockchainType = source.BlockchainType,
                FirstBlockNumber = source.FirstBlockNumber,
                BlockId = source.BlockId,
                BlockNumber = source.BlockNumber
            };
        }
    }
}
