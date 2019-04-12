using System;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Crawlers;
using NUnit.Framework;

namespace Lykke.Job.Bil2Indexer.Tests.Sql
{
    [TestFixture]
    public class CrawlerTests
    {
        [Test]
        public async Task CanSaveAndRead()
        {
            var repo = new CrawlersRepository(ContextFactory.GetPosgresTestsConnString());

            var source1 = BuildRandom();

            await repo.SaveAsync(source1);

            var retrieved1 = await repo.GetOrDefaultAsync(source1.BlockchainType, source1.Configuration);
            AssertCrawlerEquality(source1, retrieved1);

            retrieved1.RetryCurrentBlock();
            Assert.AreNotEqual(retrieved1.Sequence, source1.Sequence);

            await repo.SaveAsync(retrieved1);

            var retrieved2 = await repo.GetOrDefaultAsync(source1.BlockchainType, source1.Configuration);

            AssertCrawlerEquality(retrieved1, retrieved2);
        }

        [Test]
        public async Task CanHandlerNullStopBlock()
        {
            var repo = new CrawlersRepository(ContextFactory.GetPosgresTestsConnString());

            var source1 = Crawler.StartNew(Guid.NewGuid().ToString(), new CrawlerConfiguration(new Random(123).Next(), null));

            Assert.Null(source1.Configuration.StopAssemblingBlock);
            await repo.SaveAsync(source1);

            var retrieved1 = await repo.GetOrDefaultAsync(source1.BlockchainType, source1.Configuration);
            Assert.Null(retrieved1.Configuration.StopAssemblingBlock);
        }

        [Test]
        public async Task CanHandleNotFound()
        {
            var blockchainType = Guid.NewGuid().ToString();
            var from = new Random().Next();
            var to = new Random().Next();

            var repo = new CrawlersRepository(ContextFactory.GetPosgresTestsConnString());

            var notFound = await repo.GetOrDefaultAsync(blockchainType, new CrawlerConfiguration(from, to));
            
            Assert.IsNull(notFound);
        }

        [Test]
        public async Task HandlesOptimisticConcurrency()
        {
            var repo = new CrawlersRepository(ContextFactory.GetPosgresTestsConnString());

            var source1 = BuildRandom();

            await repo.SaveAsync(source1);
            var retrieved1 = await repo.GetOrDefaultAsync(source1.BlockchainType, new CrawlerConfiguration(source1.Configuration.StartBlock, source1.Configuration.StopAssemblingBlock));
            Assert.AreNotEqual(source1.Version, retrieved1.Version);

            retrieved1.RetryCurrentBlock();
            await repo.SaveAsync(retrieved1);

            var retrieved2 = await repo.GetOrDefaultAsync(source1.BlockchainType, new CrawlerConfiguration(source1.Configuration.StartBlock, source1.Configuration.StopAssemblingBlock));
            Assert.AreNotEqual(retrieved1.Version, retrieved2.Version);

            await repo.SaveAsync(retrieved2);

            Assert.ThrowsAsync<OptimisticConcurrencyException>(async () => { await repo.SaveAsync(retrieved1); });

            var retrieved3 = await repo.GetOrDefaultAsync(source1.BlockchainType, new CrawlerConfiguration(source1.Configuration.StartBlock, source1.Configuration.StopAssemblingBlock));
            Assert.AreNotEqual(retrieved2.Version, retrieved3.Version);

            retrieved3.RetryCurrentBlock();
            await repo.SaveAsync(retrieved3);

            Assert.ThrowsAsync<OptimisticConcurrencyException>(async () => { await repo.SaveAsync(retrieved2); });
        }

        private void AssertCrawlerEquality(Crawler source, Crawler expected)
        {
            Assert.AreEqual(source.Configuration.StartBlock, expected.Configuration.StartBlock);
            Assert.AreEqual(source.Configuration.StopAssemblingBlock, expected.Configuration.StopAssemblingBlock);
            Assert.AreEqual(source.BlockchainType, expected.BlockchainType);
            Assert.AreEqual(source.Sequence, expected.Sequence);
        }

        private Crawler BuildRandom()
        {
            var rnd = new Random();
            return new Crawler(Guid.NewGuid().ToString(), rnd.Next(), rnd.Next(), new CrawlerConfiguration(rnd.Next(), rnd.Next()), rnd.Next());
        }
    }

}
