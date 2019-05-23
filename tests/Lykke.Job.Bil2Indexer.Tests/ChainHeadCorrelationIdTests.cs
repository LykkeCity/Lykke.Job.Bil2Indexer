using Lykke.Job.Bil2Indexer.Domain;
using NUnit.Framework;

namespace Lykke.Job.Bil2Indexer.Tests
{
    [TestFixture]
    public class ChainHeadCorrelationIdTests
    {
        [Test]
        [TestCase(ChainHeadMode.CatchesCrawlerUp, 500, 300)]
        [TestCase(ChainHeadMode.FollowsCrawler, 700, 250)]
        public void Test_serialization(ChainHeadMode mode, long sequence, long crawlerSequence)
        {
            var id = new ChainHeadCorrelationId("blockchainType", mode, sequence, crawlerSequence);
            var serialized = id.ToString();
            var deserialized = ChainHeadCorrelationId.Parse(serialized);

            Assert.AreEqual(id.BlockchainType, deserialized.BlockchainType);
            Assert.AreEqual(id.Mode, deserialized.Mode);
            Assert.AreEqual(id.Sequence, deserialized.Sequence);
            Assert.AreEqual(id.CrawlerSequence, deserialized.CrawlerSequence);
        }

        [Test]
        [TestCase("ch#Bitcoin:0:100:0", "ch#Bitcoin:0:100:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:0:100:0", "ch#Bitcoin:0:101:0", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:0:101:0", "ch#Bitcoin:0:100:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:0:100:0", "ch#Bitcoin:1:101:200", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:0:101:0", "ch#Bitcoin:1:100:200", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:1:101:200", "ch#Bitcoin:0:100:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:1:100:200", "ch#Bitcoin:0:101:0", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:1:100:200", "ch#Bitcoin:1:100:200", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:1:100:200", "ch#Bitcoin:1:101:201", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:1:101:201", "ch#Bitcoin:1:100:200", ExpectedResult = false)]
        public bool Test_is_legacy_relative_to(string checkedValue, string againstValue)
        {
            var checkedId = ChainHeadCorrelationId.Parse(checkedValue);
            var againstId = ChainHeadCorrelationId.Parse(againstValue);

            return checkedId.IsLegacyRelativeTo(againstId);
        }

        [Test]
        [TestCase("ch#Bitcoin:0:100:0", "ch#Bitcoin:0:100:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:0:100:0", "ch#Bitcoin:0:101:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:0:101:0", "ch#Bitcoin:0:100:0", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:0:100:0", "ch#Bitcoin:1:101:200", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:0:101:0", "ch#Bitcoin:1:100:200", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:1:101:200", "ch#Bitcoin:0:100:0", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:1:100:200", "ch#Bitcoin:0:101:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:1:100:200", "ch#Bitcoin:1:100:200", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:1:100:200", "ch#Bitcoin:1:101:201", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:1:101:201", "ch#Bitcoin:1:100:200", ExpectedResult = true)]
        public bool Test_is_premature_relative_to(string checkedValue, string againstValue)
        {
            var checkedId = ChainHeadCorrelationId.Parse(checkedValue);
            var againstId = ChainHeadCorrelationId.Parse(againstValue);

            return checkedId.IsPrematureRelativeTo(againstId);
        }

        [Test]
        [TestCase("ch#Bitcoin:0:100:0", "ch#Bitcoin:0:100:0", ExpectedResult = false)]

        [TestCase("ch#Bitcoin:0:100:0", "ch#Bitcoin:0:101:0", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:0:100:0", "ch#Bitcoin:0:102:0", ExpectedResult = false)]

        [TestCase("ch#Bitcoin:0:101:0", "ch#Bitcoin:0:100:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:0:102:0", "ch#Bitcoin:0:100:0", ExpectedResult = false)]

        [TestCase("ch#Bitcoin:0:100:0", "ch#Bitcoin:1:101:200", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:0:100:0", "ch#Bitcoin:1:102:200", ExpectedResult = false)]

        [TestCase("ch#Bitcoin:1:101:200", "ch#Bitcoin:0:100:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:1:102:200", "ch#Bitcoin:0:100:0", ExpectedResult = false)]

        [TestCase("ch#Bitcoin:1:101:200", "ch#Bitcoin:0:100:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:1:102:200", "ch#Bitcoin:0:100:0", ExpectedResult = false)]
        
        [TestCase("ch#Bitcoin:1:100:200", "ch#Bitcoin:0:101:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:1:100:200", "ch#Bitcoin:0:102:0", ExpectedResult = false)]

        [TestCase("ch#Bitcoin:0:100:0", "ch#Bitcoin:1:101:200", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:0:100:0", "ch#Bitcoin:1:102:200", ExpectedResult = false)]

        [TestCase("ch#Bitcoin:0:101:0", "ch#Bitcoin:1:100:200", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:0:102:0", "ch#Bitcoin:1:100:200", ExpectedResult = false)]

        [TestCase("ch#Bitcoin:1:100:200", "ch#Bitcoin:1:100:200", ExpectedResult = false)]

        [TestCase("ch#Bitcoin:1:100:200", "ch#Bitcoin:1:101:201", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:1:100:200", "ch#Bitcoin:1:101:202", ExpectedResult = false)]

        [TestCase("ch#Bitcoin:1:101:201", "ch#Bitcoin:1:100:200", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:1:101:202", "ch#Bitcoin:1:100:200", ExpectedResult = false)]

        [TestCase("ch#Ripple:1:100:200", "ch#Ripple:0:101:200", ExpectedResult = true)]
        [TestCase("ch#Ripple:1:100:200", "ch#Ripple:0:101:201", ExpectedResult = false)]
        [TestCase("ch#Ripple:1:100:200", "ch#Ripple:0:101:202", ExpectedResult = false)]
        [TestCase("ch#Ripple:1:100:201", "ch#Ripple:0:101:200", ExpectedResult = false)]
        [TestCase("ch#Ripple:1:100:202", "ch#Ripple:0:101:200", ExpectedResult = false)]

        [TestCase("ch#Ripple:0:101:200", "ch#Ripple:1:100:200", ExpectedResult = false)]
        [TestCase("ch#Ripple:0:101:201", "ch#Ripple:1:100:200", ExpectedResult = false)]
        [TestCase("ch#Ripple:0:101:202", "ch#Ripple:1:100:200", ExpectedResult = false)]
        [TestCase("ch#Ripple:0:101:200", "ch#Ripple:1:100:201", ExpectedResult = false)]
        [TestCase("ch#Ripple:0:101:200", "ch#Ripple:1:100:202", ExpectedResult = false)]

        [TestCase("ch#Ripple:0:101:200", "ch#Ripple:1:100:202", ExpectedResult = false)]
        public bool Test_is_previous_of(string checkedValue, string againstValue)
        {
            var checkedId = ChainHeadCorrelationId.Parse(checkedValue);
            var againstId = ChainHeadCorrelationId.Parse(againstValue);

            return checkedId.IsPreviousOf(againstId);
        }
    }
}
