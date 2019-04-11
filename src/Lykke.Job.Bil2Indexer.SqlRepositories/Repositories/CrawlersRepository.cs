using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.IndexerState;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.IndexerState.Models;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories
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

            using (var db = new StateDataContext(_posgresConnString))
            {
                var existed = await db.Crawlers.SingleOrDefaultAsync(BuildIdPredicate(crawler.BlockchainType, crawler.Configuration.StartBlock, crawler.Configuration.StopAssemblingBlock));

                var newValues = Map(crawler);
                if (existed != null)
                {
                    db.Entry(existed).Property(nameof(CrawlerEntity.Version)).OriginalValue = crawler.Version;
                    db.Entry(existed).State = EntityState.Modified; //forces to update xmin even if actual prop is the same

                    db.Entry(existed).CurrentValues.SetValues(newValues);
                }
                else
                {
                    db.Crawlers.Add(newValues);
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
                StopAssemblingBlock = source.Configuration.StopAssemblingBlock?? StopAccemblingNullSqlMagicValue,
                StartBlock = source.Configuration.StartBlock,
                BlockchainType = source.BlockchainType,
                Sequence = source.Sequence,
                ExpectedBlockNumber = source.ExpectedBlockNumber
            };
        }
    }
}
