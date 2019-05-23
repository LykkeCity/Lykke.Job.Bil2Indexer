using System;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.BlockHeaders;
using Lykke.Job.Bil2Indexer.Tests.Sql.Mocks;
using Npgsql;
using NUnit.Framework;

namespace Lykke.Job.Bil2Indexer.Tests.Sql
{
    [TestFixture]
    public class BlockHeaderTests
    {
        [Test]
        public async Task CanSaveAndRead()
        {
            var repo = new BlockHeadersRepository(ContextFactory.GetPosgresTestsConnStringProvider());

            var source1 = BuildRandom();

            await repo.SaveAsync(source1);

            var retrieved1 = await repo.GetOrDefaultAsync(source1.BlockchainType, source1.Number);
            AssertEquals(source1, retrieved1);

            var retrieved2 = await repo.GetOrDefaultAsync(source1.BlockchainType, source1.Id);
            AssertEquals(source1, retrieved2);

            await repo.SaveAsync(retrieved1);

            var retrieved3 = await repo.GetOrDefaultAsync(source1.BlockchainType, source1.Number);

            AssertEquals(retrieved1, retrieved3);
        }

        [Test]
        public async Task CanFilter()
        {
            RemoveAll();

            var repo = new BlockHeadersRepository(ContextFactory.GetPosgresTestsConnStringProvider());

            var bType = Guid.NewGuid().ToString();

            var blocks = new[]
            {
                BuildRandom(bType, height: 1),
                BuildRandom(bType, height: 2),
                BuildRandom(bType, height: 3),
                BuildRandom(bType, height: 4),
                BuildRandom(bType, height: 5),
                BuildRandom(bType, height: 6),
            };

            foreach (var blockHeader in blocks)
            {
                await repo.SaveAsync(blockHeader);
            }

            var retrieved1 = await repo.GetCollectionAsync(bType, int.MaxValue, 999, orderAsc: true);

            Assert.AreEqual(blocks.Length, retrieved1.Count);

            var index = 0;
            foreach (var bl in retrieved1)
            {
                AssertEquals(blocks[index], bl);

                index++;
            }

            var retrieved2 = await repo.GetCollectionAsync(bType, int.MaxValue, 999, orderAsc: false);
            index = 0;
            foreach (var bl in retrieved2)
            {
                AssertEquals(blocks.Reverse().ToArray()[index], bl);

                index++;
            }

            var retrieved3 = await repo.GetCollectionAsync(bType, int.MaxValue, 2, orderAsc: false);
            Assert.AreEqual(2, retrieved3.Count);


            var retrieved4 = await repo.GetCollectionAsync(bType, int.MaxValue, 999, orderAsc: false, 
                startingAfterNumber: 6, 
                endingBeforeNumber: 1);

            Assert.AreEqual(4, retrieved4.Count);
        }


        [Test]
        public async Task CanRemove()
        {
            var repo = new BlockHeadersRepository(ContextFactory.GetPosgresTestsConnStringProvider());

            var source1 = BuildRandom();

            await repo.SaveAsync(source1);

            var retrieved1 = await repo.GetAsync(source1.BlockchainType, source1.Id);

            AssertEquals(source1, retrieved1);

            await repo.TryRemoveAsync(source1.BlockchainType, source1.Id);

            var retrieved2 = await repo.GetOrDefaultAsync(source1.BlockchainType, source1.Id);

            Assert.IsNull(retrieved2);
        }


        [Test]
        public async Task CanHandleNotFound()
        {
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
            var repo = new BlockHeadersRepository(ContextFactory.GetPosgresTestsConnStringProvider());

            var source1 = BuildRandom();

            await repo.SaveAsync(source1);
            var retrieved1 = await repo.GetOrDefaultAsync(source1.BlockchainType, source1.Id);
            Assert.AreNotEqual(source1.Version, retrieved1.Version);
            
            await repo.SaveAsync(retrieved1);

            Assert.ThrowsAsync<OptimisticConcurrencyException>(async () => { await repo.SaveAsync(source1); });

            var retrieved2 = await repo.GetOrDefaultAsync(source1.BlockchainType, source1.Id);
            Assert.AreNotEqual(retrieved1.Version, retrieved2.Version);

            await repo.SaveAsync(retrieved2);

            Assert.ThrowsAsync<OptimisticConcurrencyException>(async () => { await repo.SaveAsync(retrieved1); });

            var retrieved3 = await repo.GetOrDefaultAsync(source1.BlockchainType, source1.Id);
            Assert.AreNotEqual(retrieved2.Version, retrieved3.Version);
            
            await repo.SaveAsync(retrieved3);

            Assert.ThrowsAsync<OptimisticConcurrencyException>(async () => { await repo.SaveAsync(retrieved2); });

        }


        private void AssertEquals(BlockHeader source, BlockHeader expected)
        {
            Assert.AreEqual(source.Id, expected.Id);
            Assert.AreEqual(source.BlockchainType, expected.BlockchainType);
            Assert.AreEqual(source.Number, expected.Number);
            Assert.AreEqual(source.Size, expected.Size);
            Assert.AreEqual(source.TransactionsCount, expected.TransactionsCount);
            Assert.AreEqual(source.PreviousBlockId, expected.PreviousBlockId);
            Assert.AreEqual(source.State, expected.State);
        }

        private BlockHeader BuildRandom(string btype = null, long? height = null)
        {
            var rnd = new Random();

            var stateValues = Enum.GetValues(typeof(BlockState));
            var rdmState = (BlockState) stateValues.GetValue(rnd.Next(stateValues.Length));

            return new BlockHeader(Guid.NewGuid().ToString(), 
                0,
                btype ?? Guid.NewGuid().ToString(), 
                height ??  rnd.Next(), 
                DateTime.UtcNow + TimeSpan.FromSeconds(rnd.Next()), 
                rnd.Next(), 
                rnd.Next(), 
                Guid.NewGuid().ToString(),
                rdmState);
        }
        private void RemoveAll()
        {
            using (var conn = new NpgsqlConnection(ContextFactory.GetPosgresTestsConnString()))
            {
                conn.Execute("truncate table block_headers");
            }
        }
    }

}
