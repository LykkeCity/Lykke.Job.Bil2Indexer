//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Lykke.Common.Chaos;
//using Lykke.Job.Bil2Indexer.Contract.Events;
//using Lykke.Job.Bil2Indexer.Domain;
//using Lykke.Job.Bil2Indexer.Domain.Services;
//using Lykke.Job.Bil2Indexer.DomainServices;
//using Lykke.Job.Bil2Indexer.Tests.Mocks;
//using Moq;
//using MoreLinq;
//using NUnit.Framework;

//namespace Lykke.Job.Bil2Indexer.Tests
//{
//    // TODO: Fix ignored cases

//    [TestFixture]
//    public class ChainCrawlerTests
//    {
//        #region Chain cases

//        private static readonly ChainsCases ChainCases = new ChainsCases(new []
//        {
//            // case: 0
//            // A: 1-2-3-4-5
//            //       \
//            // B:     3-4-5-6-7
//            new Dictionary<char, BlockHeader[]>
//            {
//                {
//                    'A',
//                    new[]
//                    {
//                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
//                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
//                        new BlockHeader(3, "3A", DateTime.UtcNow, 0, 0, "2A"),
//                        new BlockHeader(4, "4A", DateTime.UtcNow, 0, 0, "3A"),
//                        new BlockHeader(5, "5A", DateTime.UtcNow, 0, 0, "4A"),
//                    }
//                },
//                {
//                    'B',
//                    new[]
//                    {
//                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
//                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
//                        new BlockHeader(3, "3B", DateTime.UtcNow, 0, 0, "2A"),
//                        new BlockHeader(4, "4B", DateTime.UtcNow, 0, 0, "3B"),
//                        new BlockHeader(5, "5B", DateTime.UtcNow, 0, 0, "4B"),
//                        new BlockHeader(6, "6B", DateTime.UtcNow, 0, 0, "5B"),
//                        new BlockHeader(7, "7B", DateTime.UtcNow, 0, 0, "6B"),
//                    }
//                }
//            },
//            // case: 1
//            // A: 1-2-3-4-5-6
//            //       \
//            // B:     3-4-5-6-7
//            //         \
//            // C:       4-5-6-7-8
//            new Dictionary<char, BlockHeader[]>
//            {
//                {
//                    'A',
//                    new[]
//                    {
//                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
//                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
//                        new BlockHeader(3, "3A", DateTime.UtcNow, 0, 0, "2A"),
//                        new BlockHeader(4, "4A", DateTime.UtcNow, 0, 0, "3A"),
//                        new BlockHeader(5, "5A", DateTime.UtcNow, 0, 0, "4A"),
//                        new BlockHeader(6, "6A", DateTime.UtcNow, 0, 0, "5A"),
//                    }
//                },
//                {
//                    'B',
//                    new[]
//                    {
//                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
//                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
//                        new BlockHeader(3, "3B", DateTime.UtcNow, 0, 0, "2A"),
//                        new BlockHeader(4, "4B", DateTime.UtcNow, 0, 0, "3B"),
//                        new BlockHeader(5, "5B", DateTime.UtcNow, 0, 0, "4B"),
//                        new BlockHeader(6, "6B", DateTime.UtcNow, 0, 0, "5B"),
//                        new BlockHeader(7, "7B", DateTime.UtcNow, 0, 0, "6B"),
//                    }
//                },
//                {
//                    'C',
//                    new[]
//                    {
//                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
//                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
//                        new BlockHeader(3, "3B", DateTime.UtcNow, 0, 0, "2A"),
//                        new BlockHeader(4, "4C", DateTime.UtcNow, 0, 0, "3B"),
//                        new BlockHeader(5, "5C", DateTime.UtcNow, 0, 0, "4C"),
//                        new BlockHeader(6, "6C", DateTime.UtcNow, 0, 0, "5C"),
//                        new BlockHeader(7, "7C", DateTime.UtcNow, 0, 0, "6C"),
//                        new BlockHeader(8, "8C", DateTime.UtcNow, 0, 0, "7C"),
//                    }
//                }
//            },
//            // case: 2
//            // A: 1-2-3-4-5-6
//            //       \
//            // B:     3-4-5-6-7
//            //           \
//            // C:         5-6-7-8
//            new Dictionary<char, BlockHeader[]>
//            {
//                {
//                    'A',
//                    new[]
//                    {
//                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
//                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
//                        new BlockHeader(3, "3A", DateTime.UtcNow, 0, 0, "2A"),
//                        new BlockHeader(4, "4A", DateTime.UtcNow, 0, 0, "3A"),
//                        new BlockHeader(5, "5A", DateTime.UtcNow, 0, 0, "4A"),
//                        new BlockHeader(6, "6A", DateTime.UtcNow, 0, 0, "5A"),
//                    }
//                },
//                {
//                    'B',
//                    new[]
//                    {
//                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
//                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
//                        new BlockHeader(3, "3B", DateTime.UtcNow, 0, 0, "2A"),
//                        new BlockHeader(4, "4B", DateTime.UtcNow, 0, 0, "3B"),
//                        new BlockHeader(5, "5B", DateTime.UtcNow, 0, 0, "4B"),
//                        new BlockHeader(6, "6B", DateTime.UtcNow, 0, 0, "5B"),
//                        new BlockHeader(7, "7B", DateTime.UtcNow, 0, 0, "6B"),
//                    }
//                },
//                {
//                    'C',
//                    new[]
//                    {
//                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
//                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
//                        new BlockHeader(3, "3B", DateTime.UtcNow, 0, 0, "2A"),
//                        new BlockHeader(4, "4B", DateTime.UtcNow, 0, 0, "3B"),
//                        new BlockHeader(5, "5C", DateTime.UtcNow, 0, 0, "4B"),
//                        new BlockHeader(6, "6C", DateTime.UtcNow, 0, 0, "5C"),
//                        new BlockHeader(7, "7C", DateTime.UtcNow, 0, 0, "6C"),
//                        new BlockHeader(8, "8C", DateTime.UtcNow, 0, 0, "7C"),
//                    }
//                }
//            },
//            // case: 3
//            // C:       4-5-6
//            //         /
//            // A: 1-2-3-4
//            //       \
//            // B:     3-4-5
//            new Dictionary<char, BlockHeader[]>
//            {
//                {
//                    'A',
//                    new[]
//                    {
//                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
//                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
//                        new BlockHeader(3, "3A", DateTime.UtcNow, 0, 0, "2A"),
//                        new BlockHeader(4, "4A", DateTime.UtcNow, 0, 0, "3A")
//                    }
//                },
//                {
//                    'B',
//                    new[]
//                    {
//                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
//                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
//                        new BlockHeader(3, "3B", DateTime.UtcNow, 0, 0, "2A"),
//                        new BlockHeader(4, "4B", DateTime.UtcNow, 0, 0, "3B"),
//                        new BlockHeader(5, "5B", DateTime.UtcNow, 0, 0, "4B")
//                    }
//                },
//                {
//                    'C',
//                    new[]
//                    {
//                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
//                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
//                        new BlockHeader(3, "3A", DateTime.UtcNow, 0, 0, "2A"),
//                        new BlockHeader(4, "4C", DateTime.UtcNow, 0, 0, "3A"),
//                        new BlockHeader(5, "5C", DateTime.UtcNow, 0, 0, "4C"),
//                        new BlockHeader(6, "6C", DateTime.UtcNow, 0, 0, "5C"),
//                    }
//                }
//            },
//            // case: 4
//            // C:     3-4-5-6
//            //       /
//            // A: 1-2-3-4
//            //       \
//            // B:     3-4-5
//            new Dictionary<char, BlockHeader[]>
//            {
//                {
//                    'A',
//                    new[]
//                    {
//                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
//                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
//                        new BlockHeader(3, "3A", DateTime.UtcNow, 0, 0, "2A"),
//                        new BlockHeader(4, "4A", DateTime.UtcNow, 0, 0, "3A")
//                    }
//                },
//                {
//                    'B',
//                    new[]
//                    {
//                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
//                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
//                        new BlockHeader(3, "3B", DateTime.UtcNow, 0, 0, "2A"),
//                        new BlockHeader(4, "4B", DateTime.UtcNow, 0, 0, "3B"),
//                        new BlockHeader(5, "5B", DateTime.UtcNow, 0, 0, "4B")
//                    }
//                },
//                {
//                    'C',
//                    new[]
//                    {
//                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
//                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
//                        new BlockHeader(3, "3C", DateTime.UtcNow, 0, 0, "2A"),
//                        new BlockHeader(4, "4C", DateTime.UtcNow, 0, 0, "3C"),
//                        new BlockHeader(5, "5C", DateTime.UtcNow, 0, 0, "4C"),
//                        new BlockHeader(6, "6C", DateTime.UtcNow, 0, 0, "5C"),
//                    }
//                }
//            },
//            // case: 5
//            // C:   2-3-4-5-6
//            //     /
//            // A: 1-2-3-4
//            //       \
//            // B:     3-4-5
//            new Dictionary<char, BlockHeader[]>
//            {
//                {
//                    'A',
//                    new[]
//                    {
//                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
//                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
//                        new BlockHeader(3, "3A", DateTime.UtcNow, 0, 0, "2A"),
//                        new BlockHeader(4, "4A", DateTime.UtcNow, 0, 0, "3A")
//                    }
//                },
//                {
//                    'B',
//                    new[]
//                    {
//                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
//                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
//                        new BlockHeader(3, "3B", DateTime.UtcNow, 0, 0, "2A"),
//                        new BlockHeader(4, "4B", DateTime.UtcNow, 0, 0, "3B"),
//                        new BlockHeader(5, "5B", DateTime.UtcNow, 0, 0, "4B")
//                    }
//                },
//                {
//                    'C',
//                    new[]
//                    {
//                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
//                        new BlockHeader(2, "2C", DateTime.UtcNow, 0, 0, "1A"),
//                        new BlockHeader(3, "3C", DateTime.UtcNow, 0, 0, "2C"),
//                        new BlockHeader(4, "4C", DateTime.UtcNow, 0, 0, "3C"),
//                        new BlockHeader(5, "5C", DateTime.UtcNow, 0, 0, "4C"),
//                        new BlockHeader(6, "6C", DateTime.UtcNow, 0, 0, "5C"),
//                    }
//                }
//            },
//            // case: 6
//            // A: 1-2-3-4
//            new Dictionary<char, BlockHeader[]>
//            {
//                {
//                    'A',
//                    new[]
//                    {
//                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
//                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
//                        new BlockHeader(3, "3A", DateTime.UtcNow, 0, 0, "2A"),
//                        new BlockHeader(4, "4A", DateTime.UtcNow, 0, 0, "3A"),
//                    }
//                }
//            },
//        });

//        #endregion


//        #region Common

//        private InMemoryBlockHeadersRepository _blockHeadersRepository;
//        private InMemoryBlocksQueue _queue;
//        private BlocksReaderApiMock _blocksReaderApi;
//        private ChainsEvaluator _chainsEvaluator;
//        private InMemoryBlocksDeduplicationRepository _blocksDeduplicationRepository;
//        private ChainCrawler _chainCrawler;
//        private Mock<IContractEventsPublisher> _contractEventsPublisher;

//        [SetUp]
//        public void SetUp()
//        {
//            _queue = new InMemoryBlocksQueue();
//            _chainsEvaluator = new ChainsEvaluator(ChainCases.Chains, _queue);
//            _blocksReaderApi = new BlocksReaderApiMock(_chainsEvaluator);
//            _blockHeadersRepository = new InMemoryBlockHeadersRepository();
//            _blocksDeduplicationRepository = new InMemoryBlocksDeduplicationRepository();
//            _contractEventsPublisher = new Mock<IContractEventsPublisher>();

//            var blockExpectationRepository = new InMemoryCrawlersRepository();
//            var chaosKitty = new SilentChaosKitty();

//            _chainCrawler = new ChainCrawler(
//                "Bitcoin", 
//                1,
//                null,
//                chaosKitty, 
//                _contractEventsPublisher.Object,
//                _blocksReaderApi, 
//                _blockHeadersRepository, 
//                blockExpectationRepository, 
//                _blocksDeduplicationRepository);

//            _queue.BlockReceived += async (s, a) =>
//            {
//                await _chainCrawler.ChooseDirectionAsync(a.Block);
//            };
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            _queue.Dispose();
//        }
        
//        #endregion


//        [Test]
//        [TestCase(0)]
//        [TestCase(1)]
//        [TestCase(2)]
//        [TestCase(3)]
//        [TestCase(4)]
//        [TestCase(5)]
//        [TestCase(6)]
//        public async Task Test_that_longest_chain_is_processed(int @case)
//        {
//            // Arrange

//            _chainsEvaluator.Case = @case;

//            // Act

//            await _chainCrawler.StartAsync();
            
//            _chainsEvaluator.Wait();
//            _queue.Stop();
//            _queue.Wait();

//            // Assert

//            var actualBlocks = await _blockHeadersRepository.GetAllAsync();
//            var expectedBlocks = ChainCases.GetLongestChain(@case);

//            Assert.IsNull(_queue.BackgroundException, _queue.BackgroundException?.ToString());

//            CollectionAssert.AreEqual(
//                expectedBlocks.Select(b => b.Number),
//                actualBlocks.Select(b => b.Number));

//            CollectionAssert.AreEqual(
//                expectedBlocks.Select(b => b.Id),
//                actualBlocks.Select(b => b.Id));

//            AssertRolledBackBlocks(@case);
//        }

//        [Test]
//        [TestCase(1, 3, "")]
//        [TestCase(1, 4, "3B,4B")]
//        [TestCase(1, 5, "3B,4B,5B")]
//        [TestCase(1, 6, "3B,4B,5B,6B")]
//        [TestCase(2, 5, "5B")]
//        [TestCase(2, 6, "5B,6B")]
//        [TestCase(3, 4, "3A,3B,4B")]
//        [TestCase(4, 4, "3B,4B")]
//        [TestCase(5, 4, "3B,4B")]
//        public async Task Test_that_chain_switching_during_backward_turn_works(int @case, long blockToSwitchToChainC, string shouldBeNotRolledBackBlockIds)
//        {
//            // Arrange

//            _chainsEvaluator.Case = @case;
//            _chainsEvaluator.ForceSwitchChain = (activeChain, blockNumber) => activeChain == 'B' && blockNumber == blockToSwitchToChainC;

//            // Act

//            await _chainCrawler.StartAsync();

//            _chainsEvaluator.Wait();
//            _queue.Stop();
//            _queue.Wait();
            
//            // Assert

//            var actualBlocks = await _blockHeadersRepository.GetAllAsync();
//            var expectedBlocks = ChainCases.GetLongestChain(@case);

//            Assert.IsNull(_queue.BackgroundException, _queue.BackgroundException?.ToString());

//            CollectionAssert.AreEqual(
//                expectedBlocks.Select(b => b.Number),
//                actualBlocks.Select(b => b.Number));

//            CollectionAssert.AreEqual(
//                expectedBlocks.Select(b => b.Id),
//                actualBlocks.Select(b => b.Id));

//            var parsedShouldBeNotRolledBackBlockIds = new HashSet<string>(shouldBeNotRolledBackBlockIds.Split(','));

//            AssertRolledBackBlocks(@case, b => !parsedShouldBeNotRolledBackBlockIds.Contains(b.Id));
//        }

//        [Test]
//        [TestCase(0, "1A")]
//        [TestCase(0, "2A")]
//        [TestCase(0, "3A")]
//        [TestCase(0, "6A")]
//        [TestCase(0, "3B")]
//        [TestCase(0, "4B")]
//        [TestCase(0, "5B")]
//        [TestCase(0, "6B")]
//        [TestCase(0, "7B")]
//        [TestCase(6, "1A")]
//        [TestCase(6, "2A")]
//        [TestCase(6, "4A")]
//        public async Task Test_that_block_duplication_is_processed_well(int @case, string duplicateBlockId)
//        {
//            // Arrange

//            _chainsEvaluator.Case = @case;

//            _chainsEvaluator.CustomBlockProcessing = (blocksQueue, chains, activeChain, block) =>
//            {
//                if (block.Id == duplicateBlockId)
//                {
//                     blocksQueue.Publish(block);
//                }

//                return true;
//            };

//            // Act

//            await _chainCrawler.StartAsync();

//            _chainsEvaluator.Wait();
//            _queue.Stop();
//            _queue.Wait();

//            // Assert

//            var actualBlocks = await _blockHeadersRepository.GetAllAsync();
//            var expectedBlocks = ChainCases.GetLongestChain(@case);

//            Assert.IsNull(_queue.BackgroundException, _queue.BackgroundException?.ToString());

//            CollectionAssert.AreEqual(
//                expectedBlocks.Select(b => b.Number),
//                actualBlocks.Select(b => b.Number));

//            CollectionAssert.AreEqual(
//                expectedBlocks.Select(b => b.Id),
//                actualBlocks.Select(b => b.Id));

//            AssertRolledBackBlocks(@case);
//        }

//        [Test]
//        [TestCase(0, "3B", "1A")]
//        [TestCase(0, "3B", "2A")]
//        [TestCase(0, "3B", "3A", Ignore = "Not supported yet case")]
//        [TestCase(0, "3B", "4A")]
//        [TestCase(0, "3B", "5A")]
//        [TestCase(0, "3B", "4B")]
//        [TestCase(0, "3B", "5B")]
//        [TestCase(0, "3B", "6B")]
//        [TestCase(0, "4B", "1A")]
//        [TestCase(0, "4B", "2A")]
//        [TestCase(0, "4B", "3A")]
//        [TestCase(0, "4B", "4A", Ignore = "Not supported yet case")]
//        [TestCase(0, "4B", "5A")]
//        [TestCase(0, "4B", "3B")]
//        [TestCase(0, "4B", "5B")]
//        [TestCase(0, "4B", "6B")]
//        [TestCase(0, "5B", "1A")]
//        [TestCase(0, "5B", "2A")]
//        [TestCase(0, "5B", "3A")]
//        [TestCase(0, "5B", "4A")]
//        [TestCase(0, "5B", "5A", Ignore = "Not supported yet case")]
//        [TestCase(0, "5B", "3B")]
//        [TestCase(0, "5B", "4B")]
//        [TestCase(0, "5B", "6B")]
//        [TestCase(6, "2A", "3A")]
//        [TestCase(6, "2A", "4A")]
//        public async Task Test_that_disordered_blocks_eventually_processed(int @case, string substitutableBlockId, string substituteBlockId)
//        {
//            // Arrange

//            _chainsEvaluator.Case = @case;

//            var isSubstituted = false;

//            _chainsEvaluator.CustomBlockProcessing = (blocksQueue, chains, activeChain, block) =>
//            {
//                if (!isSubstituted && block.Id == substitutableBlockId)
//                {
//                    isSubstituted = true;

//                    var chain = chains[substituteBlockId.Last()];
//                    var substituteBlock = chain.First(b => b.Id == substituteBlockId);

//                    Console.WriteLine($"Substituting: {block} with {substituteBlock}");

//                    blocksQueue.Publish(substituteBlock);
//                }

//                return true;
//            };

//            // Act

//            await _chainCrawler.StartAsync();

//            _chainsEvaluator.Wait();
//            _queue.Stop();
//            _queue.Wait();

//            // Assert

//            var actualBlocks = await _blockHeadersRepository.GetAllAsync();
//            var expectedBlocks = ChainCases.GetLongestChain(@case);

//            Assert.IsNull(_queue.BackgroundException, _queue.BackgroundException?.ToString());

//            Assert.IsTrue(isSubstituted);

//            CollectionAssert.AreEqual(
//                expectedBlocks.Select(b => b.Number),
//                actualBlocks.Select(b => b.Number));

//            CollectionAssert.AreEqual(
//                expectedBlocks.Select(b => b.Id),
//                actualBlocks.Select(b => b.Id));

//            AssertRolledBackBlocks(@case);
//        }

//        private void AssertRolledBackBlocks(int @case, Func<BlockHeader, bool> predicate = null)
//        {
//            var chains = ChainCases.Chains[@case];
//            var expectedRolledBackBlocks = new List<BlockHeader>();
            
//            foreach (var chainKey in chains.Keys.Skip(1).OrderByDescending(k => k))
//            {
//                var chain = chains[chainKey];
//                var previousChain = chains[(char) (chainKey - 1)];

//                var exceptedBlocks = previousChain.ExceptBy(chain, b => b.Id);

//                if (predicate != null)
//                {
//                    exceptedBlocks = exceptedBlocks.Where(predicate);
//                }

//                expectedRolledBackBlocks.AddRange(exceptedBlocks);
//            }

//            var blocksThatWasNotRolledBack = chains.Values
//                .Aggregate((a, b) => a.Concat(b).ToArray())
//                .ExceptBy(expectedRolledBackBlocks, b => b.Id);

//            foreach (var blockId in expectedRolledBackBlocks.Select(b => b.Id).Distinct())
//            {
//                _contractEventsPublisher.Verify(
//                    x => x.PublishAsync(It.Is<BlockRolledBackEvent>(b => b.BlockId == blockId)),
//                    Times.Exactly(expectedRolledBackBlocks.Count(b => b.Id == blockId)));
//            }

//            foreach (var blockId in blocksThatWasNotRolledBack.Select(b => b.Id).Distinct())
//            {
//                _contractEventsPublisher.Verify(
//                    x => x.PublishAsync(It.Is<BlockRolledBackEvent>(b => b.BlockId == blockId)),
//                    Times.Never);
//            }
//        }
//    }
//}
