using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Contract;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Services;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.BalanceActions;
using Lykke.Job.Bil2Indexer.Tests.Sql.Mocks;
using Lykke.Logs;
using Lykke.Numerics;
using Moq;
using NUnit.Framework;

namespace Lykke.Job.Bil2Indexer.Tests.Sql
{
    [TestFixture]
    public class BalanceActionsTests
    {
        [Test]
        public async Task CanSaveAndRead()
        {


            var address = BuildRandmomAddress();
            var asset = BuildRandmomAsset();
            var scale = new Random().Next(0, 15);
            var bType = Guid.NewGuid().ToString();
            
            var actions = new List<BalanceAction>();
            var max = 99;
            var ctr = 0;

            var sum = Money.Parse("0");
            do
            {
                var act = BuildRandomBalanceAction(asset, address, scale);
                actions.Add(act);

                sum = Money.Add(sum, act.Amount);
                ctr++;
            } while (ctr <= max);

            var repo = new BalanceActionsRepository(ContextFactory.GetPosgresTestsConnString(),
                BuildProviderMock(asset,bType, scale).Object);

            await repo.AddIfNotExistsAsync(bType, actions);
            await repo.AddIfNotExistsAsync(bType, actions);

            var retrievedSum = await repo.GetBalanceAsync(bType, address, asset, int.MaxValue);

            Assert.AreEqual(sum, retrievedSum);

            var byTx = await repo.GetSomeOfBalancesAsync(bType, actions.Select(p => p.TransactionId).ToHashSet());

            Assert.AreEqual(actions.Count, byTx.Count);

            foreach (var balanceAction in actions)
            {
                var retrieved = byTx[balanceAction.TransactionId];

                Assert.AreEqual(balanceAction.AccountId, retrieved.Keys.Single());
                Assert.AreEqual(balanceAction.Amount, retrieved.Values.Single());
            }

            var allAssets = await repo.GetBalancesAsync(bType, address, long.MaxValue);

            Assert.AreEqual(1, allAssets.Count);

            Assert.AreEqual(asset, allAssets.Keys.Single());
        }

        private BalanceAction BuildRandomBalanceAction(Asset asset, Address address, int scale)
        {
            var rdm = new Random();
            return new BalanceAction(new AccountId(address, asset), new Money(new BigInteger(rdm.Next()), scale),
                rdm.Next(1, 123333), new BlockId(Guid.NewGuid().ToString()),
                new TransactionId(Guid.NewGuid().ToString()));
        }
        private Asset BuildRandmomAsset()
        {
            return new Asset(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }

        private Address BuildRandmomAddress()
        {
            return new Address(Guid.NewGuid().ToString());
        }

        private Mock<IAssetInfosProvider> BuildProviderMock(Asset asset, string blockchainType, int scale)
        {
            var result = new Mock<IAssetInfosProvider>();
            var assetInfo = new AssetInfo(blockchainType, asset, scale);

            result.Setup(p => p.GetAsync(It.IsAny<string>(), It.Is<Asset>(z=> asset.Id == z.Id))).ReturnsAsync(assetInfo);

            result.Setup(p => p.GetSomeOfAsync(It.IsAny<string>(), It.IsAny<IEnumerable<Asset>>()))
                .ReturnsAsync(new[] {assetInfo});

            return result;
        }
    }
}
