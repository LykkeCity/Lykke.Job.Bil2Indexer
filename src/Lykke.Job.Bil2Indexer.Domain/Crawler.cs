using System;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class Crawler
    {
        public string BlockchainType { get; }
        public long Version { get; }
        public CrawlerConfiguration Configuration { get; }
        public long Sequence { get; private set; }
        public long ExpectedBlockNumber { get; private set; }
        public bool IsCompleted => ExpectedBlockNumber == Configuration.StopAssemblingBlock;

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

        public long EvaluateNextBlockToMoveForward(BlockHeader newBlock)
        {
            return newBlock.Number + 1;
        }

        public void MoveTo(long nextBlockNumber)
        {
            ++Sequence;
            ExpectedBlockNumber = nextBlockNumber;
        }

        public CrawlerCorrelationId GetCorrelationId()
        {
            return new CrawlerCorrelationId(BlockchainType, Configuration, Sequence);
        }

        public override string ToString()
        {
            var configuration = Configuration.ToString();

            return $"{BlockchainType}:{configuration}({Sequence}):{ExpectedBlockNumber}";
        }

        public bool IsOnBlock(long blockNumber)
        {
            return ExpectedBlockNumber - 1 == blockNumber;
        }
    }
}
