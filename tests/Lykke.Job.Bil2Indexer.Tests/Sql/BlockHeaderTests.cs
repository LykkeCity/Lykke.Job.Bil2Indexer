using System;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.BlockHeaders;
using Lykke.Job.Bil2Indexer.Tests.Sql.Mocks;
using NUnit.Framework;

namespace Lykke.Job.Bil2Indexer.Tests.Sql
{
    [TestFixture]
    public class BlockHeaderTests
    {
        [Test]
        public async Task CanSaveAndRead()
        {
            var repo = new BlockHeadersRepository(ContextFactory.GetPosgresTestsConnString());

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
        public async Task CanRemove()
        {
            var repo = new BlockHeadersRepository(ContextFactory.GetPosgresTestsConnString());

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

            var repo = new BlockHeadersRepository(ContextFactory.GetPosgresTestsConnString());

            var notFound1 = await repo.GetOrDefaultAsync(blockchainType, blockId);

            Assert.IsNull(notFound1);

            var notFound2 = await repo.GetOrDefaultAsync(blockchainType, blockNumber);

            Assert.IsNull(notFound2);

            Assert.ThrowsAsync<InvalidOperationException>(async () => { await repo.GetAsync(blockchainType, blockId); });
        }


        [Test]
        public async Task HandlesOptimisticConcurrency()
        {
            var repo = new BlockHeadersRepository(ContextFactory.GetPosgresTestsConnString());

            var source1 = BuildRandom();

            await repo.SaveAsync(source1);
            var retrieved1 = await repo.GetOrDefaultAsync(source1.BlockchainType, source1.Id);
            Assert.AreNotEqual(source1.Version, retrieved1.Version);
            
            await repo.SaveAsync(retrieved1);

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

        private BlockHeader BuildRandom()
        {
            var rnd = new Random();

            var stateValues = Enum.GetValues(typeof(BlockState));
            var rdmState = (BlockState) stateValues.GetValue(rnd.Next(stateValues.Length));

            return new BlockHeader(Guid.NewGuid().ToString(), 
                rnd.Next(), 
                Guid.NewGuid().ToString(), rnd.Next(), 
                DateTime.UtcNow + TimeSpan.FromSeconds(rnd.Next()), 
                rnd.Next(), 
                rnd.Next(), 
                Guid.NewGuid().ToString(),
                rdmState);
        }
    }

}
