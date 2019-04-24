using Lykke.Job.Bil2Indexer.Domain;
using NUnit.Framework;

namespace Lykke.Job.Bil2Indexer.Tests
{
    [TestFixture]
    public class ChainHeadCorrelationIdTests
    {
        [Test]
        public void Test_serialization()
        {
            var id = new ChainHeadCorrelationId("blockchainType", 5);
            var serialized = id.ToString();
            var deserialized = ChainHeadCorrelationId.Parse(serialized);

            Assert.AreEqual(id.BlockchainType, deserialized.BlockchainType);
            Assert.AreEqual(id.Sequence, deserialized.Sequence);
        }
    }
}