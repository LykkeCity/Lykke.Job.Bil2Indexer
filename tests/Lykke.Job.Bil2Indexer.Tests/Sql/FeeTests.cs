using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Common;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.FeeEnvelopes;
using Lykke.Job.Bil2Indexer.Tests.Sql.Mocks;
using Lykke.Numerics;
using NUnit.Framework;

namespace Lykke.Job.Bil2Indexer.Tests.Sql
{
    [TestFixture]
    public class FeeTests
    {
        [Test]

        public async Task CanSave()
        {
            var btype = Guid.NewGuid().ToString();
            var scale = new Random().Next(1, 15);
            var blockId = Guid.NewGuid().ToString();
            var asset = new Asset(Guid.NewGuid().ToString("N"));

            var fees = new List<FeeEnvelope>();
            var max = 99;
            var count = 0;

            do
            {
                fees.Add(BuildRandmon(btype, scale, blockId, asset));
                count++;

            } while (count<=max);

            var repo = new FeeEnvelopesRepository(ContextFactory.GetPosgresTestsConnString());

            await repo.AddIfNotExistsAsync(fees);

            do
            {
                fees.Add(BuildRandmon(btype, scale, blockId, asset));
                count++;

            } while (count <= max*2);

            await repo.AddIfNotExistsAsync(fees);
            await repo.AddIfNotExistsAsync(fees);


            foreach (var feeEnvelope in fees)
            {
                var retrieved = await repo.GetAsync(feeEnvelope.BlockchainType, feeEnvelope.TransactionId,
                    feeEnvelope.Fee.Asset);

                AssertEquals(feeEnvelope, retrieved);
            }

            var retrieved2  = await repo.GetBlockFeesAsync(btype, blockId, 10000, null);

            Assert.Null(retrieved2.Continuation);

            Assert.AreEqual(fees.Count, retrieved2.Items.Count);

            foreach (var feeEnvelope in fees)
            {
                var retrieved = retrieved2.Items.Single(p => p.TransactionId == feeEnvelope.TransactionId);

                AssertEquals(feeEnvelope, retrieved);
            }
        }

        public FeeEnvelope BuildRandmon(string blockchainType, int scale, string blockId, Asset asset)
        {
            var rdnm = new Random();
            return new FeeEnvelope(blockchainType,
                blockId,
                Guid.NewGuid().ToString(), 
                new Fee(asset,
                    new UMoney(new BigInteger(double.MaxValue - rdnm.Next(1, int.MaxValue)), scale)));
        }

        private void AssertEquals(FeeEnvelope a, FeeEnvelope b)
        {
            Assert.AreEqual(a.ToJson(), b.ToJson());
        }
    }
}
