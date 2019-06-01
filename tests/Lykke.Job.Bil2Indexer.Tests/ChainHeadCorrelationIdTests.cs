using Lykke.Job.Bil2Indexer.Domain;
using NUnit.Framework;

namespace Lykke.Job.Bil2Indexer.Tests
{
    [TestFixture]
    public class ChainHeadCorrelationIdTests
    {
        [Test]
        [TestCase(10, 500, 300)]
        public void Test_serialization(long modeSequence, long blockSequence, long crawlerSequence)
        {
            var id = new ChainHeadCorrelationId("blockchainType", modeSequence, blockSequence, crawlerSequence);
            var serialized = id.ToString();
            var deserialized = ChainHeadCorrelationId.Parse(serialized);

            Assert.AreEqual(id.BlockchainType, deserialized.BlockchainType);
            Assert.AreEqual(id.ModeSequence, deserialized.ModeSequence);
            Assert.AreEqual(id.BlockSequence, deserialized.BlockSequence);
            Assert.AreEqual(id.CrawlerSequence, deserialized.CrawlerSequence);
        }

        [Test]
        
        // The same
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:50:100:0", ExpectedResult = false)]

        // AttachToCrawler:
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:51:100:40", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:55:100:40", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:51:100:40", "ch#Bitcoin:50:100:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:55:100:40", "ch#Bitcoin:50:100:0", ExpectedResult = false)]

        // DetachFromCrawler
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:51:100:0", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:55:100:0", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:51:100:0", "ch#Bitcoin:50:100:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:55:100:0", "ch#Bitcoin:50:100:0", ExpectedResult = false)]

        // ExtendTo in catching up mode
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:50:101:0", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:50:105:0", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:50:101:0", "ch#Bitcoin:50:100:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:50:105:0", "ch#Bitcoin:50:100:0", ExpectedResult = false)]

        // ExtendTo in following mode / ReduceTo
        [TestCase("ch#Bitcoin:50:100:40", "ch#Bitcoin:50:101:41", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:50:100:40", "ch#Bitcoin:50:105:45", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:50:101:41", "ch#Bitcoin:50:100:40", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:50:105:45", "ch#Bitcoin:50:100:40", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:50:100:41", "ch#Bitcoin:50:100:40", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:50:100:45", "ch#Bitcoin:50:100:40", ExpectedResult = false)]
        public bool Test_is_legacy_relative_to(string checkedValue, string againstValue)
        {
            var checkedId = ChainHeadCorrelationId.Parse(checkedValue);
            var againstId = ChainHeadCorrelationId.Parse(againstValue);

            return checkedId.IsLegacyRelativeTo(againstId);
        }

        [Test]

        // The same
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:50:100:0", ExpectedResult = false)]

        // AttachToCrawler:
        [TestCase("ch#Bitcoin:51:100:40", "ch#Bitcoin:50:100:0", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:55:100:40", "ch#Bitcoin:50:100:0", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:51:100:40", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:55:100:40", ExpectedResult = false)]

        // DetachFromCrawler
        [TestCase("ch#Bitcoin:51:100:0", "ch#Bitcoin:50:100:0", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:55:100:0", "ch#Bitcoin:50:100:0", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:51:100:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:55:100:0", ExpectedResult = false)]

        // ExtendTo in catching up mode
        [TestCase("ch#Bitcoin:50:101:0", "ch#Bitcoin:50:100:0", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:50:105:0", "ch#Bitcoin:50:100:0", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:50:101:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:50:105:0", ExpectedResult = false)]

        // ExtendTo in following mode / ReduceTo
        [TestCase("ch#Bitcoin:50:101:41", "ch#Bitcoin:50:100:40", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:50:105:45", "ch#Bitcoin:50:100:40", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:50:100:41", "ch#Bitcoin:50:100:40", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:50:100:45", "ch#Bitcoin:50:100:40", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:50:100:40", "ch#Bitcoin:50:101:41", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:50:100:40", "ch#Bitcoin:50:105:45", ExpectedResult = false)]
        public bool Test_is_premature_relative_to(string checkedValue, string againstValue)
        {
            var checkedId = ChainHeadCorrelationId.Parse(checkedValue);
            var againstId = ChainHeadCorrelationId.Parse(againstValue);

            return checkedId.IsPrematureRelativeTo(againstId);
        }

        [Test]
        // The same
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:50:100:0", ExpectedResult = false)]

        // AttachToCrawler:
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:51:100:40", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:55:100:40", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:51:100:40", "ch#Bitcoin:50:100:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:55:100:40", "ch#Bitcoin:50:100:0", ExpectedResult = false)]

        // DetachFromCrawler
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:51:100:0", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:55:100:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:51:100:0", "ch#Bitcoin:50:100:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:55:100:0", "ch#Bitcoin:50:100:0", ExpectedResult = false)]

        // ExtendTo in catching up mode
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:50:101:0", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:50:100:0", "ch#Bitcoin:50:105:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:50:101:0", "ch#Bitcoin:50:100:0", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:50:105:0", "ch#Bitcoin:50:100:0", ExpectedResult = false)]

        // ExtendTo in following mode / ReduceTo
        [TestCase("ch#Bitcoin:50:100:40", "ch#Bitcoin:50:101:41", ExpectedResult = true)]
        [TestCase("ch#Bitcoin:50:100:40", "ch#Bitcoin:50:105:45", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:50:101:41", "ch#Bitcoin:50:100:40", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:50:105:45", "ch#Bitcoin:50:100:40", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:50:100:41", "ch#Bitcoin:50:100:40", ExpectedResult = false)]
        [TestCase("ch#Bitcoin:50:100:45", "ch#Bitcoin:50:100:40", ExpectedResult = false)]

        public bool Test_is_previous_of(string checkedValue, string againstValue)
        {
            var checkedId = ChainHeadCorrelationId.Parse(checkedValue);
            var againstId = ChainHeadCorrelationId.Parse(againstValue);

            return checkedId.IsPreviousOf(againstId);
        }
    }
}
