﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Common;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Coins;
using Lykke.Job.Bil2Indexer.Tests.Sql.Mocks;
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

            var coins = new List<Coin>();
            var max = 33;
            var count = 0;

            do
            {
                coins.Add(GenerateRandom(bType));
                count++;

            } while (count <= max);

            await repo.AddIfNotExistsAsync(coins);
            await repo.AddIfNotExistsAsync(coins);

            do
            {
                coins.Add(GenerateRandom(bType));
                count++;

            } while (count <= max);

            await repo.AddIfNotExistsAsync(coins);


            var retrieved = await repo.GetSomeOfAsync(bType, coins.Select(p => p.Id).ToList());


            Assert.AreEqual(coins.Count, retrieved.Count);

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
                GenerateRandom(bType),

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


            var idsToDelete = coins.Take(5).Select(p => p.Id).ToList();

            await repo.RemoveIfExistAsync(bType, idsToDelete.Select(p => new TransactionId(p.TransactionId)).ToHashSet());
            await repo.RemoveIfExistAsync(bType, idsToDelete.Select(p => new TransactionId(p.TransactionId)).ToHashSet());

            var retrieved2 = await repo.GetSomeOfAsync(bType, ids);


            Assert.AreEqual(retrieved2.Count, coins.Length - idsToDelete.Count);


            Assert.AreEqual(retrieved2.Count(p => idsToDelete.Contains(p.Id)), 0);


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
                GenerateRandom(bType),

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

            var idsToSpend = coins.Take(4).Select(p => p.Id).ToList();

            await repo.SpendAsync(bType, idsToSpend);
            await repo.SpendAsync(bType, idsToSpend);


            var retrieved2 = await repo.GetSomeOfAsync(bType, ids);

            Assert.True(retrieved2.Where(p=>idsToSpend.Contains(p.Id)).All(p => p.IsSpent));
            Assert.True(retrieved2.Where(p => !idsToSpend.Contains(p.Id)).All(p => !p.IsSpent));


            await repo.RemoveIfExistAsync(bType, coins.Select(p => new TransactionId(p.Id.TransactionId)).ToHashSet());

            Assert.ThrowsAsync<ArgumentException>(() =>
                repo.SpendAsync(bType, coins.Skip(4).Take(3).Select(p => p.Id).ToList()));
        }

        private void AssertEquals(Coin a, Coin b)
        {
            Assert.AreEqual(a.ToJson(), b.ToJson());
        }

        private Coin GenerateRandom(string blockchainType)
        {
            var rdm = new Random();

            return new Coin
            (
                blockchainType,
                new CoinId(Guid.NewGuid().ToString(), rdm.Next()),
                new Asset(Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString()),
                new UMoney(new BigInteger(long.MaxValue - rdm.Next()), 0),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                AddressTagType.Number,
                null,
                false,
                Guid.NewGuid().ToString(),
                rdm.Next()
            );
        }
    }
}
