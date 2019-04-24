using Lykke.Job.Bil2Indexer.Domain;
using NUnit.Framework;

namespace Lykke.Job.Bil2Indexer.Tests
{
    [TestFixture]
    public class CrawlerCorrelationIdTests
    {
        [Test]
        public void Test_serialization()
        {
            var id = new CrawlerCorrelationId("blockchainType", new CrawlerConfiguration(1, 10), 5);
            var serialized = id.ToString();
            var deserialized = CrawlerCorrelationId.Parse(serialized);

            Assert.AreEqual(id.BlockchainType, deserialized.BlockchainType);
            Assert.AreEqual(id.Configuration.StartBlock, deserialized.Configuration.StartBlock);
            Assert.AreEqual(id.Configuration.StopAssemblingBlock, deserialized.Configuration.StopAssemblingBlock);
            Assert.AreEqual(id.Sequence, deserialized.Sequence);
        }
    }
}
