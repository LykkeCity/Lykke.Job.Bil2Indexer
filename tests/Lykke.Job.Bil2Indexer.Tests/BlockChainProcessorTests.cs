using System;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Bil2.Contract.BlocksReader.Commands;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.DomainServices;
using Lykke.Job.Bil2Indexer.Tests.Mocks;
using Moq;
using NUnit.Framework;

namespace Lykke.Job.Bil2Indexer.Tests
{
    [TestFixture]
    public class BlockChainProcessorTests
    {
        [Test]
        public async Task Test_that_chain_switching_works()
        {
            var chainA = new[]
            {
                new BlockHeader(1, "1A", DateTime.UtcNow, 0, 0, null),
                new BlockHeader(2, "2A", DateTime.UtcNow, 0, 0, "1A"),
                new BlockHeader(3, "3A", DateTime.UtcNow, 0, 0, "2A"),
                new BlockHeader(4, "4A", DateTime.UtcNow, 0, 0, "3A"),
                new BlockHeader(5, "5A", DateTime.UtcNow, 0, 0, "4A"),
            };

            var chainB = new[]
            {
                new BlockHeader(3, "3B", DateTime.UtcNow, 0, 0, "2A"),
                new BlockHeader(4, "4B", DateTime.UtcNow, 0, 0, "3B"),
                new BlockHeader(5, "5B", DateTime.UtcNow, 0, 0, "4B"),
                new BlockHeader(6, "6B", DateTime.UtcNow, 0, 0, "5B"),
                new BlockHeader(7, "7B", DateTime.UtcNow, 0, 0, "6B"),
            };

            var blocksRepository = new InMemoryBlocksRepository();
            var blocksReaderApiMock = new Mock<IBlocksReaderApi>();
            var processor = new BlocksProcessor(blocksReaderApiMock.Object, blocksRepository);
            var activeChain = 'A';

            blocksReaderApiMock
                .Setup(x => x.SendAsync(It.IsAny<ReadBlockCommand>()))
                .Returns<ReadBlockCommand>(async command =>
                {
                    BlockHeader block = null;

                    if (activeChain == 'A')
                    {
                        block = chainA.FirstOrDefault(b => b.Number == command.BlockNumber);

                        if (block == null)
                        {
                            activeChain = 'B';
                        }
                    } 
                    
                    if (activeChain == 'B')
                    {
                        block = chainB.FirstOrDefault(b => b.Number == command.BlockNumber);
                    }

                    if (block != null)
                    {
                        await processor.ProcessBlockAsync(block);
                    }
                });

            await blocksReaderApiMock.Object.SendAsync(new ReadBlockCommand(1));

            var storedBlocks = await blocksRepository.GetAllAsync();

            CollectionAssert.AreEqual(
                new[] {1, 2, 3, 4, 5, 6, 7},
                storedBlocks.Select(b => b.Number));

            CollectionAssert.AreEqual(
                new[] {"1A", "2A", "3B", "4B", "5B", "6B", "7B"}, 
                storedBlocks.Select(b => b.Number));
        }
    }
}
