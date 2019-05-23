using System;
using System.Threading.Tasks;
using Dapper;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.BlockHeaders;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.ChainHeads;
using Lykke.Job.Bil2Indexer.Tests.Sql.Mocks;
using Npgsql;
using NUnit.Framework;

namespace Lykke.Job.Bil2Indexer.Tests.Sql
{
    [TestFixture]
    public class ChainHeadTests
    {
        [Test]
        public async Task CanSaveAndRead()
        {
            RemoveAll();

            var repo = new ChainHeadsRepository(ContextFactory.GetPosgresTestsConnStringProvider());

            var source1 = BuildRandom();

            await repo.SaveAsync(source1);

            var retrieved1 = await repo.GetOrDefaultAsync(source1.BlockchainType);
            AssertEquals(source1, retrieved1);

            var crawler = new Crawler(source1.BlockchainType, 0, 100, new CrawlerConfiguration(0, null), 101);

            retrieved1.ExtendTo((retrieved1.BlockNumber ?? 0) + 1, Guid.NewGuid().ToString(), crawler);

            await repo.SaveAsync(retrieved1);

            var retrieved2= await repo.GetOrDefaultAsync(source1.BlockchainType);
            AssertEquals(retrieved1, retrieved2);
            
            await repo.SaveAsync(retrieved2);

            var retrieved3 = await repo.GetOrDefaultAsync(source1.BlockchainType);

            AssertEquals(retrieved1, retrieved3);
        }


        [Test]
        public async Task CanHandleNotFound()
        {
            RemoveAll();

            var blockchainType = Guid.NewGuid().ToString();
            var blockNumber = new Random().Next();
            var blockId = Guid.NewGuid().ToString();

            var repo = new BlockHeadersRepository(ContextFactory.GetPosgresTestsConnStringProvider());

            var notFound1 = await repo.GetOrDefaultAsync(blockchainType, blockId);

            Assert.IsNull(notFound1);

            var notFound2 = await repo.GetOrDefaultAsync(blockchainType, blockNumber);

            Assert.IsNull(notFound2);

            Assert.ThrowsAsync<InvalidOperationException>(async () => { await repo.GetAsync(blockchainType, blockId); });
        }


        [Test]
        public async Task HandlesOptimisticConcurrency()
        {
            RemoveAll();

            var repo = new ChainHeadsRepository(ContextFactory.GetPosgresTestsConnStringProvider());

            var source1 = BuildRandom();

            await repo.SaveAsync(source1);
            var retrieved1 = await repo.GetOrDefaultAsync(source1.BlockchainType);
            Assert.AreNotEqual(source1.Version, retrieved1.Version);

            Assert.ThrowsAsync<OptimisticConcurrencyException>(async () => { await repo.SaveAsync(source1); });

            await repo.SaveAsync(retrieved1);

            var retrieved2 = await repo.GetOrDefaultAsync(source1.BlockchainType);
            Assert.AreNotEqual(retrieved1.Version, retrieved2.Version);

            await repo.SaveAsync(retrieved2);

            Assert.ThrowsAsync<OptimisticConcurrencyException>(async () => { await repo.SaveAsync(retrieved1); });

            var retrieved3 = await repo.GetOrDefaultAsync(source1.BlockchainType);
            Assert.AreNotEqual(retrieved2.Version, retrieved3.Version);
            
            await repo.SaveAsync(retrieved3);

            Assert.ThrowsAsync<OptimisticConcurrencyException>(async () => { await repo.SaveAsync(retrieved2); });
        }


        private void AssertEquals(ChainHead source, ChainHead expected)
        {
            Assert.AreEqual(source.BlockchainType, expected.BlockchainType);
            Assert.AreEqual(source.FirstBlockNumber, expected.FirstBlockNumber);
            Assert.AreEqual(source.BlockNumber, expected.BlockNumber);
            Assert.AreEqual(source.BlockId, expected.BlockId);
        }

        private static ChainHead BuildRandom()
        {
            var rnd = new Random();

            return new ChainHead
            (
                Guid.NewGuid().ToString(),
                rnd.Next(),
                0,
                0,
                0,
                rnd.Next(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                ChainHeadMode.FollowsCrawler
            );
        }

        private void RemoveAll()
        {
            using (var conn = new NpgsqlConnection(ContextFactory.GetPosgresTestsConnString()))
            {
                conn.Execute("truncate table chain_heads");
            }
        }
    }

}
