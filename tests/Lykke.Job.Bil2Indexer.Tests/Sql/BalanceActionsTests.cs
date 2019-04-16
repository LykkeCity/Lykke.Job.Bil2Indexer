using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.BalanceActions;
using Lykke.Logs;
using Lykke.Numerics;
using NUnit.Framework;

namespace Lykke.Job.Bil2Indexer.Tests.Sql
{
    [TestFixture]
    public class BalanceActionsTests
    {
        [Test]
        public async Task CanSaveAndRead()
        {
            var repo = new BalanceActionsRepository(ContextFactory.GetPosgresTestsConnString(),
                EmptyLogFactory.Instance);

            var address = BuildRandmomAddress();
            var asset = BuildRandmomAsset();
            var scale = 8;
            var bType = Guid.NewGuid().ToString();
            
            var actions = new List<BalanceAction>();
            var max = 99;
            var ctr = 0;

            do
            {
                actions.Add(BuildRandomBalanceAction(asset, address, scale));
                ctr++;
            } while (ctr<=max);

            await repo.AddIfNotExistAsync(bType, actions);
            await repo.AddIfNotExistAsync(bType, actions);
        }

        private BalanceAction BuildRandomBalanceAction(Asset asset, Address address, int scale)
        {
            var rdm = new Random();
            return new BalanceAction(address, asset, new Money(new BigInteger(rdm.Next()), scale ), rdm.Next(1, 99999), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }
        private Asset BuildRandmomAsset()
        {
            return new Asset(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
        }

        private Address BuildRandmomAddress()
        {
            return new Address(Guid.NewGuid().ToString());
        }
    }
}
