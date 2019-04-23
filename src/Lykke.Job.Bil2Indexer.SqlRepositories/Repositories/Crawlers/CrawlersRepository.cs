using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.IndexerState;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.IndexerState.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Crawlers
{
    public class CrawlersRepository:ICrawlersRepository
    {
        private readonly string _posgresConnString;
        private const long StopAccemblingNullSqlMagicValue = -1;

        public CrawlersRepository(string posgresConnString)
        {
            _posgresConnString = posgresConnString;
        }

        public async Task<Crawler> GetOrDefaultAsync(string blockchainType, CrawlerConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            using (var db = new StateDataContext(_posgresConnString))
            {
                var existed = await db.Crawlers.SingleOrDefaultAsync(BuildIdPredicate(blockchainType,
                    configuration.StartBlock, configuration.StopAssemblingBlock));

                return existed != null ? Map(existed) : null;
            }
        }

        public async Task SaveAsync(Crawler crawler)
        {
            if (crawler.Configuration == null)
            {
                throw new ArgumentNullException(nameof(crawler.Configuration));
            }

            var dbEntity = Map(crawler);
            var isExisted = crawler.Version != 0;

            using (var db = new StateDataContext(_posgresConnString))
            {
                if (isExisted)
                {
                    db.Crawlers.Update(dbEntity);

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
                    await db.Crawlers.AddAsync(dbEntity);

                    try
                    {
                        await db.SaveChangesAsync();
                    }
                    catch (PostgresException e) when (e.IsUniqueConstraintViolationException())
                    {
                        throw new OptimisticConcurrencyException(e);
                    }
                }

            }
        }

        private Expression<Func<CrawlerEntity, bool>> BuildIdPredicate(string blockchainType, long startBlock, long? stopAccemblingBlock)
        {
            var mappedStop = stopAccemblingBlock ?? StopAccemblingNullSqlMagicValue;

            return p =>
                p.BlockchainType == blockchainType
                && p.StartBlock == startBlock
                && p.StopAssemblingBlock == mappedStop;
        }

        private Crawler Map(CrawlerEntity source)
        {
            return new Crawler(blockchainType:source.BlockchainType, 
                version:source.Version, 
                sequence:source.Sequence, 
                configuration:new CrawlerConfiguration(source.StartBlock, source.StopAssemblingBlock != StopAccemblingNullSqlMagicValue? (long?) source.StopAssemblingBlock:null),
                expectedBlockNumber: source.ExpectedBlockNumber);
        }

        private CrawlerEntity Map(Crawler source)
        {
            return new CrawlerEntity
            {
                Version = source.Version,
                StopAssemblingBlock = source.Configuration.StopAssemblingBlock ?? StopAccemblingNullSqlMagicValue,
                StartBlock = source.Configuration.StartBlock,
                BlockchainType = source.BlockchainType,
                Sequence = source.Sequence,
                ExpectedBlockNumber = source.ExpectedBlockNumber
            };
        }
    }
}
