using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Coins;
using Lykke.Logs;
using Lykke.Numerics;
using NUnit.Framework;

namespace Lykke.Job.Bil2Indexer.Tests.Sql
{
    [TestFixture]
    public class CoinTests
    {
        [Test]
        public async Task CanSaveAndRead()
        {
            var repo = new CoinsRepository(ContextFactory.GetPosgresTestsConnString(), EmptyLogFactory.Instance);

            var bType = Guid.NewGuid().ToString();

            var coins = new[]
            {
                GenerateRandom(bType),
                GenerateRandom(bType),
                GenerateRandom(bType),
                GenerateRandom(bType),
                GenerateRandom(bType)
            };

            await repo.AddIfNotExistsAsync(coins);
            await repo.AddIfNotExistsAsync(coins);
            await repo.AddIfNotExistsAsync(coins);


            var retrieved = await repo.GetSomeOfAsync(bType, coins.Select(p => p.Id).ToList());


            Assert.AreEqual(coins.Length, retrieved.Count);

            foreach (var coin in retrieved)
            {
                AssertEquals(coin, coins.Single(p=>p.Id == coin.Id));
            }


        }

        [Test]
        public async Task CanDelete()
        {
            var repo = new CoinsRepository(ContextFactory.GetPosgresTestsConnString(), EmptyLogFactory.Instance);

            var bType = Guid.NewGuid().ToString();

            var coins = new[]
            {
                GenerateRandom(bType),
                GenerateRandom(bType),
                GenerateRandom(bType),
                GenerateRandom(bType),
                GenerateRandom(bType)
            };

            var ids = coins.Select(p => p.Id).ToList();

            await repo.AddIfNotExistsAsync(coins);


            var retrieved = await repo.GetSomeOfAsync(bType, ids);


            Assert.AreEqual(coins.Length, retrieved.Count);

            await repo.RemoveIfExistAsync(bType, ids.Select(p => new TransactionId(p.TransactionId)).ToHashSet());
            await repo.RemoveIfExistAsync(bType, ids.Select(p => new TransactionId(p.TransactionId)).ToHashSet());

            var retrieved2 = await repo.GetSomeOfAsync(bType, ids);


            Assert.AreEqual(retrieved2.Count, 0);
        }



        [Test]
        public async Task CanSpend()
        {
            var repo = new CoinsRepository(ContextFactory.GetPosgresTestsConnString(), EmptyLogFactory.Instance);

            var bType = Guid.NewGuid().ToString();

            var coins = new[]
            {
                GenerateRandom(bType),
                GenerateRandom(bType),
                GenerateRandom(bType),
                GenerateRandom(bType),
                GenerateRandom(bType)
            };

            var ids = coins.Select(p => p.Id).ToList();

            await repo.AddIfNotExistsAsync(coins);
            
            var retrieved = await repo.GetSomeOfAsync(bType, ids);


            Assert.AreEqual(coins.Length, retrieved.Count);
            Assert.True(retrieved.All(p => !p.IsSpent));

            await repo.SpendAsync(bType, ids);
            await repo.SpendAsync(bType, ids);


            var retrieved2 = await repo.GetSomeOfAsync(bType, ids);
            Assert.True(retrieved2.All(p => p.IsSpent));
        }

        private void AssertEquals(Coin a, Coin b)
        {
            Assert.AreEqual(a.Address, b.Address);
            Assert.AreEqual(a.Id, b.Id);
            Assert.AreEqual(a.AddressNonce, b.AddressNonce);
            Assert.AreEqual(a.AddressTag, b.AddressTag);
            Assert.AreEqual(a.AddressTagType, b.AddressTagType);
            Assert.AreEqual(a.Asset, b.Asset);
            Assert.AreEqual(a.IsSpent, b.IsSpent);
            Assert.AreEqual(a.BlockchainType, b.BlockchainType);
            //Assert.AreEqual(a.Value, b.Value);

        }

        private Coin GenerateRandom(string blockchainType)
        {
            var rdm = new Random();

            return new Coin(blockchainType, new CoinId(Guid.NewGuid().ToString(), rdm.Next()), new Asset(Guid.NewGuid().ToString(), Guid.NewGuid().ToString()), new UMoney(new BigInteger(rdm.Next()), 0), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), AddressTagType.Number, null, false);
        }
    }
}
