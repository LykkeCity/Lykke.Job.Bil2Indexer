using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Commands;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.DomainServices;
using Lykke.Job.Bil2Indexer.Tests.Mocks;
using NUnit.Framework;

namespace Lykke.Job.Bil2Indexer.Tests
{
    [TestFixture]
    public class BlocksProcessorTests
    {
        #region Chain cases

        private static readonly Dictionary<char, BlockHeader[]>[] Chains =
        {
            // case: 0
            // A: 1-2-3-4-5
            //       \
            // B:     3-4-5-6-7
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
                    }
                },
                {
                    'B',
                    new[]
                    {
                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
                        new BlockHeader(3, "3B", DateTime.UtcNow, 0, 0, "2A"),
                        new BlockHeader(4, "4B", DateTime.UtcNow, 0, 0, "3B"),
                        new BlockHeader(5, "5B", DateTime.UtcNow, 0, 0, "4B"),
                        new BlockHeader(6, "6B", DateTime.UtcNow, 0, 0, "5B"),
                        new BlockHeader(7, "7B", DateTime.UtcNow, 0, 0, "6B"),
                    }
                }
            },
            // case: 1
            // A: 1-2-3-4-5-6
            //       \
            // B:     3-4-5-6-7
            //         \
            // C:       4-5-6-7-8
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
                    }
                },
                {
                    'B',
                    new[]
                    {
                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
                        new BlockHeader(3, "3B", DateTime.UtcNow, 0, 0, "2A"),
                        new BlockHeader(4, "4B", DateTime.UtcNow, 0, 0, "3B"),
                        new BlockHeader(5, "5B", DateTime.UtcNow, 0, 0, "4B"),
                        new BlockHeader(6, "6B", DateTime.UtcNow, 0, 0, "5B"),
                        new BlockHeader(7, "7B", DateTime.UtcNow, 0, 0, "6B"),
                    }
                },
                {
                    'C',
                    new[]
                    {
                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
                        new BlockHeader(3, "3B", DateTime.UtcNow, 0, 0, "2A"),
                        new BlockHeader(4, "4C", DateTime.UtcNow, 0, 0, "3B"),
                        new BlockHeader(5, "5C", DateTime.UtcNow, 0, 0, "4C"),
                        new BlockHeader(6, "6C", DateTime.UtcNow, 0, 0, "5C"),
                        new BlockHeader(7, "7C", DateTime.UtcNow, 0, 0, "6C"),
                        new BlockHeader(8, "8C", DateTime.UtcNow, 0, 0, "7C"),
                    }
                }
            },
            // case: 2
            // A: 1-2-3-4-5-6
            //       \
            // B:     3-4-5-6-7
            //           \
            // C:         5-6-7-8
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
                    }
                },
                {
                    'B',
                    new[]
                    {
                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
                        new BlockHeader(3, "3B", DateTime.UtcNow, 0, 0, "2A"),
                        new BlockHeader(4, "4B", DateTime.UtcNow, 0, 0, "3B"),
                        new BlockHeader(5, "5B", DateTime.UtcNow, 0, 0, "4B"),
                        new BlockHeader(6, "6B", DateTime.UtcNow, 0, 0, "5B"),
                        new BlockHeader(7, "7B", DateTime.UtcNow, 0, 0, "6B"),
                    }
                },
                {
                    'C',
                    new[]
                    {
                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
                        new BlockHeader(3, "3B", DateTime.UtcNow, 0, 0, "2A"),
                        new BlockHeader(4, "4B", DateTime.UtcNow, 0, 0, "3B"),
                        new BlockHeader(5, "5C", DateTime.UtcNow, 0, 0, "4B"),
                        new BlockHeader(6, "6C", DateTime.UtcNow, 0, 0, "5C"),
                        new BlockHeader(7, "7C", DateTime.UtcNow, 0, 0, "6C"),
                        new BlockHeader(8, "8C", DateTime.UtcNow, 0, 0, "7C"),
                    }
                }
            },
            // case: 3
            // C:       4-5-6
            //         /
            // A: 1-2-3-4
            //       \
            // B:     3-4-5
            new Dictionary<char, BlockHeader[]>
            {
                {
                    'A',
                    new[]
                    {
                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
                        new BlockHeader(3, "3A", DateTime.UtcNow, 0, 0, "2A"),
                        new BlockHeader(4, "4A", DateTime.UtcNow, 0, 0, "3A")
                    }
                },
                {
                    'B',
                    new[]
                    {
                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
                        new BlockHeader(3, "3B", DateTime.UtcNow, 0, 0, "2A"),
                        new BlockHeader(4, "4B", DateTime.UtcNow, 0, 0, "3B"),
                        new BlockHeader(5, "5B", DateTime.UtcNow, 0, 0, "4B")
                    }
                },
                {
                    'C',
                    new[]
                    {
                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
                        new BlockHeader(3, "3A", DateTime.UtcNow, 0, 0, "2A"),
                        new BlockHeader(4, "4C", DateTime.UtcNow, 0, 0, "3A"),
                        new BlockHeader(5, "5C", DateTime.UtcNow, 0, 0, "4C"),
                        new BlockHeader(6, "6C", DateTime.UtcNow, 0, 0, "5C"),
                    }
                }
            },
            // case: 4
            // C:     3-4-5-6
            //       /
            // A: 1-2-3-4
            //       \
            // B:     3-4-5
            new Dictionary<char, BlockHeader[]>
            {
                {
                    'A',
                    new[]
                    {
                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
                        new BlockHeader(3, "3A", DateTime.UtcNow, 0, 0, "2A"),
                        new BlockHeader(4, "4A", DateTime.UtcNow, 0, 0, "3A")
                    }
                },
                {
                    'B',
                    new[]
                    {
                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
                        new BlockHeader(3, "3B", DateTime.UtcNow, 0, 0, "2A"),
                        new BlockHeader(4, "4B", DateTime.UtcNow, 0, 0, "3B"),
                        new BlockHeader(5, "5B", DateTime.UtcNow, 0, 0, "4B")
                    }
                },
                {
                    'C',
                    new[]
                    {
                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
                        new BlockHeader(3, "3C", DateTime.UtcNow, 0, 0, "2A"),
                        new BlockHeader(4, "4C", DateTime.UtcNow, 0, 0, "3C"),
                        new BlockHeader(5, "5C", DateTime.UtcNow, 0, 0, "4C"),
                        new BlockHeader(6, "6C", DateTime.UtcNow, 0, 0, "5C"),
                    }
                }
            },
            // case: 5
            // C:   2-3-4-5-6
            //     /
            // A: 1-2-3-4
            //       \
            // B:     3-4-5
            new Dictionary<char, BlockHeader[]>
            {
                {
                    'A',
                    new[]
                    {
                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
                        new BlockHeader(3, "3A", DateTime.UtcNow, 0, 0, "2A"),
                        new BlockHeader(4, "4A", DateTime.UtcNow, 0, 0, "3A")
                    }
                },
                {
                    'B',
                    new[]
                    {
                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
                        new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
                        new BlockHeader(3, "3B", DateTime.UtcNow, 0, 0, "2A"),
                        new BlockHeader(4, "4B", DateTime.UtcNow, 0, 0, "3B"),
                        new BlockHeader(5, "5B", DateTime.UtcNow, 0, 0, "4B")
                    }
                },
                {
                    'C',
                    new[]
                    {
                        new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
                        new BlockHeader(2, "2C", DateTime.UtcNow, 0, 0, "1A"),
                        new BlockHeader(3, "3C", DateTime.UtcNow, 0, 0, "2C"),
                        new BlockHeader(4, "4C", DateTime.UtcNow, 0, 0, "3C"),
                        new BlockHeader(5, "5C", DateTime.UtcNow, 0, 0, "4C"),
                        new BlockHeader(6, "6C", DateTime.UtcNow, 0, 0, "5C"),
                    }
                }
            },
            // case: 6
            // A: 1-2-3-4
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
                    }
                }
            },
        };

        #endregion


        #region Common

        private InMemoryBlocksRepository _blocksRepository;
        private InMemoryReadBlockCommandsQueue _queue;
        private BlocksReaderApiMock _blocksReaderApi;
        private ChainsEvaluator _chainsEvaluator;
        private InMemoryBlocksDeduplicationRepository _blocksDeduplicationRepository;

        [SetUp]
        public void SetUp()
        {
            _blocksRepository = new InMemoryBlocksRepository();
            _blocksDeduplicationRepository = new InMemoryBlocksDeduplicationRepository();
            _queue = new InMemoryReadBlockCommandsQueue();
            _blocksReaderApi = new BlocksReaderApiMock(_queue);
            
            var blocksProcessor = new BlocksProcessor(1, _blocksReaderApi, _blocksRepository, _blocksDeduplicationRepository);

            _chainsEvaluator = new ChainsEvaluator(Chains, blocksProcessor);

            _queue.CommandReceived += async (s, a) =>
            {
                if (!await _chainsEvaluator.EvaluateBlockAsync(a.Command.BlockNumber))
                {
                    _queue.Stop();
                }
            };
        }

        [TearDown]
        public void TearDown()
        {
            _queue.Dispose();
        }

        private static BlockHeader[] GetLongestChain(int @case)
        {
            return Chains[@case].Values.OrderByDescending(x => x.Length).First();
        }

        #endregion


        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public async Task Test_that_longest_chain_is_processed(int @case)
        {
            // Arrange

            _chainsEvaluator.Case = @case;

            // Act

            await _blocksReaderApi.SendAsync(new ReadBlockCommand(1));

            _queue.Wait();

            // Assert

            var actualBlocks = await _blocksRepository.GetAllAsync();
            var expectedBlocks = GetLongestChain(@case);

            Assert.IsNull(_queue.BackgroundException, _queue.BackgroundException?.ToString());

            CollectionAssert.AreEqual(
                expectedBlocks.Select(b => b.Number),
                actualBlocks.Select(b => b.Number));

            CollectionAssert.AreEqual(
                expectedBlocks.Select(b => b.Hash),
                actualBlocks.Select(b => b.Hash));
        }

        [Test]
        [TestCase(1, 3)]
        [TestCase(1, 4)]
        [TestCase(1, 5)]
        [TestCase(1, 6)]
        [TestCase(2, 5)]
        [TestCase(2, 6)]
        [TestCase(3, 4)]
        [TestCase(4, 4)]
        [TestCase(5, 4)]
        public async Task Test_that_chain_switching_during_backward_turn_works(int @case, long blockToSwitchToChainC)
        {
            // Arrange

            _chainsEvaluator.Case = @case;
            _chainsEvaluator.ForceSwitchChain = (activeChain, blockNumber) => activeChain == 'B' && blockNumber == blockToSwitchToChainC;

            // Act

            await _blocksReaderApi.SendAsync(new ReadBlockCommand(1));

            if (!_queue.Wait())
            {
                _queue.Stop();
                _queue.Wait();
            }

            // Assert

            var actualBlocks = await _blocksRepository.GetAllAsync();
            var expectedBlocks = GetLongestChain(@case);

            Assert.IsNull(_queue.BackgroundException, _queue.BackgroundException?.ToString());

            CollectionAssert.AreEqual(
                expectedBlocks.Select(b => b.Number),
                actualBlocks.Select(b => b.Number));

            CollectionAssert.AreEqual(
                expectedBlocks.Select(b => b.Hash),
                actualBlocks.Select(b => b.Hash));
        }

        [Test]
        [TestCase(0, "1A")]
        [TestCase(0, "2A")]
        [TestCase(0, "3A")]
        [TestCase(0, "6A")]
        [TestCase(0, "3B")]
        [TestCase(0, "4B")]
        [TestCase(0, "5B")]
        [TestCase(0, "6B")]
        [TestCase(0, "7B")]
        [TestCase(6, "1A")]
        [TestCase(6, "2A")]
        [TestCase(6, "4A")]
        public async Task Test_that_block_duplication_is_processed_well(int @case, string duplicateBlockHash)
        {
            // Arrange

            _chainsEvaluator.Case = @case;

            _chainsEvaluator.CustomBlockProcessing = async (blockProcessor, chains, activeChain, block) =>
            {
                if (block.Hash == duplicateBlockHash)
                {
                    await blockProcessor.ProcessBlockAsync(block);
                }

                return true;
            };

            // Act

            await _blocksReaderApi.SendAsync(new ReadBlockCommand(1));

            if (!_queue.Wait())
            {
                _queue.Stop();
                _queue.Wait();
            }

            // Assert

            var actualBlocks = await _blocksRepository.GetAllAsync();
            var expectedBlocks = GetLongestChain(@case);

            Assert.IsNull(_queue.BackgroundException, _queue.BackgroundException?.ToString());

            CollectionAssert.AreEqual(
                expectedBlocks.Select(b => b.Number),
                actualBlocks.Select(b => b.Number));

            CollectionAssert.AreEqual(
                expectedBlocks.Select(b => b.Hash),
                actualBlocks.Select(b => b.Hash));
        }

        [Test]
        [TestCase(0, "3B", "1A")]
        [TestCase(0, "3B", "2A")]
        [TestCase(0, "3B", "3A", Ignore = "Not supported yet case")]
        [TestCase(0, "3B", "4A")]
        [TestCase(0, "3B", "5A")]
        [TestCase(0, "3B", "4B")]
        [TestCase(0, "3B", "5B")]
        [TestCase(0, "3B", "6B")]
        [TestCase(0, "4B", "1A")]
        [TestCase(0, "4B", "2A")]
        [TestCase(0, "4B", "3A")]
        [TestCase(0, "4B", "4A", Ignore = "Not supported yet case")]
        [TestCase(0, "4B", "5A")]
        [TestCase(0, "4B", "3B")]
        [TestCase(0, "4B", "5B")]
        [TestCase(0, "4B", "6B")]
        [TestCase(0, "5B", "1A")]
        [TestCase(0, "5B", "2A")]
        [TestCase(0, "5B", "3A")]
        [TestCase(0, "5B", "4A")]
        [TestCase(0, "5B", "5A", Ignore = "Not supported yet case")]
        [TestCase(0, "5B", "3B")]
        [TestCase(0, "5B", "4B")]
        [TestCase(0, "5B", "6B")]
        [TestCase(6, "2A", "3A")]
        [TestCase(6, "2A", "4A")]
        public async Task Test_that_disordered_blocks_eventually_processed(int @case, string substitutableBlockHash, string substituteBlockHash)
        {
            // Arrange

            _chainsEvaluator.Case = @case;

            var isSubstituted = false;

            _chainsEvaluator.CustomBlockProcessing = async (blockProcessor, chains, activeChain, block) =>
            {
                if (!isSubstituted && block.Hash == substitutableBlockHash)
                {
                    isSubstituted = true;

                    var chain = chains[substituteBlockHash.Last()];
                    var substituteBlock = chain.First(b => b.Hash == substituteBlockHash);

                    Console.WriteLine($"Substituting: {block} with {substituteBlock}");

                    await blockProcessor.ProcessBlockAsync(substituteBlock);
                }

                return true;
            };

            // Act

            await _blocksReaderApi.SendAsync(new ReadBlockCommand(1));

            if (!_queue.Wait())
            {
                _queue.Stop();
                _queue.Wait();
            }

            // Assert

            var actualBlocks = await _blocksRepository.GetAllAsync();
            var expectedBlocks = GetLongestChain(@case);

            Assert.IsNull(_queue.BackgroundException, _queue.BackgroundException?.ToString());

            Assert.IsTrue(isSubstituted);

            CollectionAssert.AreEqual(
                expectedBlocks.Select(b => b.Number),
                actualBlocks.Select(b => b.Number));

            CollectionAssert.AreEqual(
                expectedBlocks.Select(b => b.Hash),
                actualBlocks.Select(b => b.Hash));
        }
    }
}
