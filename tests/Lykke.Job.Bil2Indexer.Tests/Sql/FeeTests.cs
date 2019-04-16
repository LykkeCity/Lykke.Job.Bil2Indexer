using System;
using System.Numerics;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.FeeEnvelopes;
using Lykke.Logs;
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
            var fees = new[]
            {
                BuildRandmon(btype, scale),
                BuildRandmon(btype, scale),
                BuildRandmon(btype, scale),
                BuildRandmon(btype, scale),
                BuildRandmon(btype, scale),
                BuildRandmon(btype, scale),
                BuildRandmon(btype, scale),
                BuildRandmon(btype, scale),
                BuildRandmon(btype, scale),
                BuildRandmon(btype, scale),
                BuildRandmon(btype, scale),
                BuildRandmon(btype, scale),
                BuildRandmon(btype, scale)
            };

            var repo = new FeeEnvelopesRepository(ContextFactory.GetPosgresTestsConnString(), EmptyLogFactory.Instance);

            await repo.AddIfNotExistsAsync(fees);
            await repo.AddIfNotExistsAsync(fees);
            await repo.AddIfNotExistsAsync(fees);


            foreach (var feeEnvelope in fees)
            {
                var retrieved = await repo.GetAsync(feeEnvelope.BlockchainType, feeEnvelope.TransactionId,
                    feeEnvelope.Fee.Asset);

                AssertEquals(feeEnvelope, retrieved);
            }
        }

        public FeeEnvelope BuildRandmon(string blockchainType, int scale)
        {
            var rdnm = new Random();
            return new FeeEnvelope(blockchainType, 
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(), 
                new Fee(new Asset(Guid.NewGuid().ToString()),
                    new UMoney(new BigInteger(rdnm.Next(1, 99999)), scale)));
        }

        private void AssertEquals(FeeEnvelope a, FeeEnvelope b)
        {
            Assert.AreEqual(a.BlockId, b.BlockId);
            Assert.AreEqual(a.BlockchainType, b.BlockchainType);
            Assert.AreEqual(a.TransactionId, b.TransactionId);
            Assert.AreEqual(a.Fee, b.Fee);
        }
    }
}
