using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Crawlers
{
    public class CrawlersRepository:ICrawlersRepository
    {
        private readonly IPgConnectionStringProvider _connectionStringProvider;

        public CrawlersRepository(IPgConnectionStringProvider connectionStringProvider)
        {
            _connectionStringProvider = connectionStringProvider;
        }

        private const long StopAssemblingNullSqlMagicValue = -1;
        

        public async Task<Crawler> GetOrDefaultAsync(string blockchainType, CrawlerConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            using (var db = new DataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var existed = await db.Crawlers.SingleOrDefaultAsync(BuildIdPredicate(configuration.StartBlock,
                    configuration.StopAssemblingBlock));

                return existed != null ? Map(existed, blockchainType) : null;
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

            using (var db = new DataContext(_connectionStringProvider.GetConnectionString(crawler.BlockchainType)))
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
                    catch (DbUpdateException e) when (e.IsNaturalKeyViolationException())
                    {
                        throw new OptimisticConcurrencyException(e);
                    }
                }

            }
        }

        private static Expression<Func<CrawlerEntity, bool>> BuildIdPredicate(long startBlock, long? stopAssemblingBlock)
        {
            var mappedStop = stopAssemblingBlock ?? StopAssemblingNullSqlMagicValue;

            return p =>p.StartBlock == startBlock
                && p.StopAssemblingBlock == mappedStop;
        }

        private static Crawler Map(CrawlerEntity source, string blockchainType)
        {
            return new Crawler(blockchainType: blockchainType, 
                version:source.Version, 
                sequence:source.Sequence, 
                configuration:new CrawlerConfiguration(source.StartBlock, source.StopAssemblingBlock != StopAssemblingNullSqlMagicValue? (long?) source.StopAssemblingBlock:null),
                expectedBlockNumber: source.ExpectedBlockNumber);
        }

        private static CrawlerEntity Map(Crawler source)
        {
            return new CrawlerEntity
            {
                Version = source.Version,
                StopAssemblingBlock = source.Configuration.StopAssemblingBlock ?? StopAssemblingNullSqlMagicValue,
                StartBlock = source.Configuration.StartBlock,
                Sequence = source.Sequence,
                ExpectedBlockNumber = source.ExpectedBlockNumber
            };
        }
    }
}
