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
            }
        };

        #endregion


        #region Common

        private InMemoryBlocksRepository _blocksRepository;
        private InMemoryReadBlockCommandsQueue _queue;
        private BlocksReaderApiMock _blocksReaderApi;
        private BlocksProcessor _blocksProcessor;
        private ChainsEvaluator _chainsEvaluator;

        [SetUp]
        public void SetUp()
        {
            _blocksRepository = new InMemoryBlocksRepository();
            _queue = new InMemoryReadBlockCommandsQueue();
            _blocksReaderApi = new BlocksReaderApiMock(_queue);
            _blocksProcessor = new BlocksProcessor(_blocksReaderApi, _blocksRepository);
            _chainsEvaluator = new ChainsEvaluator(Chains, _blocksProcessor);

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
        public async Task Test_that_chain_switching_during_forward_turn_works(int @case)
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
    }
}
