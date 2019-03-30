using System;
using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class Crawler
    {
        public string BlockchainType { get; }
        public long Version { get; }
        public CrawlerConfiguration Configuration { get; }
        public long Sequence { get; private set; }
        public long ExpectedBlockNumber { get; private set; }

        public Crawler(
            string blockchainType,
            long version,
            long sequence,
            CrawlerConfiguration configuration,
            long expectedBlockNumber)
        {
            BlockchainType = blockchainType;
            Version = version;
            Sequence = sequence;
            Configuration = configuration;
            ExpectedBlockNumber = expectedBlockNumber;
        }

        public static Crawler StartNew(
            string blockchainType,
            CrawlerConfiguration configuration)
        {
            return new Crawler(
                blockchainType: blockchainType,
                version: 0,
                sequence: 0,
                configuration:configuration,
                expectedBlockNumber: configuration.StartBlock);
        }

        public CrawlingDirection EvaluateDirection(BlockHeader previousBlock, BlockHeader newBlock)
        {
            if (newBlock.Number != ExpectedBlockNumber)
            {
                throw new InvalidOperationException($"Disordered block: [{newBlock.Number}], expected block: [{ExpectedBlockNumber}]");
            }

            return previousBlock == null || newBlock.PreviousBlockId == previousBlock.Id
                ? CrawlingDirection.Forward
                : CrawlingDirection.Backward;
        }

        public async Task<long> EvaluateNextBlockToMoveForwardAsync(
            BlockHeader newBlock,
            Func<long, Task<BlockHeader>> getBlockHeader,
            Action<BlockHeader> rollbackBlock)
        {
            var nextBlockNumber = newBlock.Number;
            var currentBlock = newBlock;

            while (true)
            {
                nextBlockNumber++;

                if (!Configuration.CanProcess(nextBlockNumber))
                {
                    break;
                }

                var nextBlock = await getBlockHeader.Invoke(nextBlockNumber);

                if (nextBlock == null)
                {
                    break;
                }

                // TODO: Log this as warn

                // Removes already stored blocks, which belongs to another chain.
                // For example, if chain was switched during the backward turn, thus
                // already read on the backward turn blocks are belongs to the stale chain.

                if (nextBlock.PreviousBlockId != currentBlock.Id)
                {
                    rollbackBlock.Invoke(nextBlock);
                }

                currentBlock = nextBlock;
            }

            return nextBlockNumber;
        }

        public long EvaluateNextBlockToMoveBackward(BlockHeader newBlock, BlockHeader previousBlock, Action<BlockHeader> rollbackBlock)
        {
            rollbackBlock.Invoke(previousBlock);

            // it's possible that previous block can't be processed by the crawler (because of crawler range bounds),
            // but we ignores this case since this is very unlikely that chain fork can intersect ranges of different crawlers.
            return newBlock.Number - 1;
        }

        public void MoveTo(long nextBlockNumber)
        {
            ++Sequence;
            ExpectedBlockNumber = nextBlockNumber;
        }

        public void RetryCurrentBlock()
        {
            ++Sequence;
        }

        public CrawlerCorrelationId GetCorrelationId()
        {
            return new CrawlerCorrelationId(Configuration, Sequence);
        }

        public override string ToString()
        {
            var configuration = Configuration.ToString();

            return $"{BlockchainType}:{configuration}({Sequence}):{ExpectedBlockNumber}";
        }
    }
}
