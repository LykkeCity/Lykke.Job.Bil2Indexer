using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Common.Chaos;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Services;
using Lykke.Job.Bil2Indexer.DomainServices;
using Lykke.Job.Bil2Indexer.Tests.Mocks;
using Moq;
using NUnit.Framework;

namespace Lykke.Job.Bil2Indexer.Tests
{
    [TestFixture]
    public class MultipleChainCrawlersTests
    {
        #region Chain cases

        private static readonly ChainsCases ChainCases = new ChainsCases(new[]
        {
            // case: 0
            // A: 1-2-3-4-5-6-7-8-9-10-11-12-13-14-15
            new Dictionary<char, BlockHeader[]>
            {
                {
                    'A',
                    new[]
                    {
                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
                        new BlockHeader(3, "3A", DateTime.UtcNow, 0, 0, "2A"),
                        new BlockHeader(4, "4A", DateTime.UtcNow, 0, 0, "3A"),
                        new BlockHeader(5, "5A", DateTime.UtcNow, 0, 0, "4A"),
                        new BlockHeader(6, "6A", DateTime.UtcNow, 0, 0, "5A"),
                        new BlockHeader(7, "7A", DateTime.UtcNow, 0, 0, "6A"),
                        new BlockHeader(8, "8A", DateTime.UtcNow, 0, 0, "7A"),
                        new BlockHeader(9, "9A", DateTime.UtcNow, 0, 0, "8A"),
                        new BlockHeader(10, "10A", DateTime.UtcNow, 0, 0, "9A"),
                        new BlockHeader(11, "11A", DateTime.UtcNow, 0, 0, "10A"),
                        new BlockHeader(12, "12A", DateTime.UtcNow, 0, 0, "11A"),
                        new BlockHeader(13, "13A", DateTime.UtcNow, 0, 0, "12A"),
                        new BlockHeader(14, "14A", DateTime.UtcNow, 0, 0, "13A"),
                        new BlockHeader(15, "15A", DateTime.UtcNow, 0, 0, "14A")
                    }
                }
            }
        });

        #endregion

        #region Common

        private InMemoryBlockHeadersRepository _blockHeadersRepository;
        private InMemoryBlocksQueue _queue;
        private BlocksReaderApiMock _blocksReaderApi;
        private ChainsEvaluator _chainsEvaluator;
        private InMemoryBlocksDeduplicationRepository _blocksDeduplicationRepository;
        private Mock<IContractEventsPublisher> _contractEventsPublisher;
        private InMemoryBlockExpectationRepository _blockExpectationRepository;
        private IChaosKitty _chaosKitty;

        [SetUp]
        public void SetUp()
        {
            _queue = new InMemoryBlocksQueue();
            _chainsEvaluator = new ChainsEvaluator(ChainCases.Chains, _queue);
            _blocksReaderApi = new BlocksReaderApiMock(_chainsEvaluator);
            _blockHeadersRepository = new InMemoryBlockHeadersRepository();
            _blocksDeduplicationRepository = new InMemoryBlocksDeduplicationRepository();
            _contractEventsPublisher = new Mock<IContractEventsPublisher>();
            _blockExpectationRepository = new InMemoryBlockExpectationRepository();
            _chaosKitty = new SilentChaosKitty();
        }

        [TearDown]
        public void TearDown()
        {
            _queue.Dispose();
        }

        #endregion


        [Test]
        [TestCase(0, "1-6,6-11,11-")]
        [TestCase(0, "6-11,11-,1-6")]
        [TestCase(0, "1-4,4-8,8-12,12-")]
        public async Task Test(int @case, string crawlerRanges)
        {
            // Arrange

            var parsedCrawlerRanges = crawlerRanges
                .Split(',')
                .Select(x => x.Split('-'))
                .Select(x => new
                {
                    Start = long.Parse(x[0]),
                    Stop = string.IsNullOrWhiteSpace(x[1]) 
                        ? default(long?)
                        : (long?)long.Parse(x[1]),
                })
                .Select(x => new
                {
                    x.Start,
                    x.Stop,
                    Id = $"{x.Start}-{x.Stop}"
                })
                .ToArray();

            var crawlers = parsedCrawlerRanges
                .Select(x => new ChainCrawler
                (
                    "Bitcoin",
                    x.Start,
                    x.Stop,
                    _chaosKitty,
                    _contractEventsPublisher.Object,
                    _blocksReaderApi,
                    _blockHeadersRepository,
                    _blockExpectationRepository,
                    _blocksDeduplicationRepository
                ))
                .ToArray();

            var actualReceivedBlocks = new List<BlockHeader>();

            _queue.BlockReceived += async (s, a) =>
            {
                actualReceivedBlocks.Add(a.Block);

                foreach (var crawler in crawlers)
                {
                    await crawler.ProcessBlockAsync(a.Block);
                }
            };

            // Act

            foreach (var crawler in crawlers)
            {
                await crawler.StartAsync();
            }

            // Assert

            var actualChainBlocks = await _blockHeadersRepository.GetAllAsync();
            var expectedChainBlocks = ChainCases.GetLongestChain(@case);
            var expectedReceivedBlocks = new List<BlockHeader>();
            
            for (var i = 0; i < expectedChainBlocks.Length; ++i)
            {
                foreach (var range in parsedCrawlerRanges)
                {
                    var block = expectedChainBlocks.SingleOrDefault(b => b.Number == range.Start + i);

                    if (block != null && block.Number >= range.Start && (!range.Stop.HasValue || block.Number < range.Stop.Value))
                    {
                        expectedReceivedBlocks.Add(block);
                    }
                }
            }

            var expectedLastNextBlockToReadNumbers = parsedCrawlerRanges
                .ToDictionary(r => r.Id, r => r.Stop ?? expectedChainBlocks.Last().Number + 1);

            var waitForCrawlersTasks = parsedCrawlerRanges
                .Select(r => Task.Run(async () =>
                {
                    while (true)
                    {
                        var expectedBlock = await _blockExpectationRepository.GetOrDefaultAsync(r.Id);

                        if (expectedBlock != null)
                        {
                            if (expectedLastNextBlockToReadNumbers[r.Id] == expectedBlock.Number)
                            {
                                break;
                            }
                        }
                    }
                }))
                .ToArray();

            _chainsEvaluator.Wait();

            Task.WaitAll(waitForCrawlersTasks, Waiting.Timeout);

            _queue.Stop();
            _queue.Wait();

            var actualLastNextBlockToReadNumbers = parsedCrawlerRanges
                .ToDictionary(
                    r => r.Id,
                    r => _blockExpectationRepository.GetOrDefaultAsync(r.Id)
                        .ConfigureAwait(false)
                        .GetAwaiter()
                        .GetResult()
                        ?.Number);

            foreach (var range in parsedCrawlerRanges)
            {
                Assert.AreEqual(expectedLastNextBlockToReadNumbers[range.Id], actualLastNextBlockToReadNumbers[range.Id], $"Range: {range.Id}, unexpected last next block to read");
            }

            Assert.IsNull(_queue.BackgroundException, _queue.BackgroundException?.ToString());

            CollectionAssert.AreEqual(
                expectedChainBlocks.Select(b => b.Number),
                actualChainBlocks.Select(b => b.Number));

            CollectionAssert.AreEqual(
                expectedChainBlocks.Select(b => b.Id),
                actualChainBlocks.Select(b => b.Id));

            CollectionAssert.AreEqual(
                expectedReceivedBlocks.Select(b => b.Number),
                actualReceivedBlocks.Select(b => b.Number));

            CollectionAssert.AreEqual(
                expectedReceivedBlocks.Select(b => b.Id),
                actualReceivedBlocks.Select(b => b.Id));
        }
    }
}
