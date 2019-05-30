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
        public CrawlerMode Mode { get; private set; }

        public bool IsWaitingForChainHead => Mode == CrawlerMode.WaitingForChainHead;
        public bool IsIndexing => Mode == CrawlerMode.Indexing;

        public Crawler(
            string blockchainType,
            long version,
            long sequence,
            CrawlerConfiguration configuration,
            long expectedBlockNumber,
            CrawlerMode mode)
        {
            BlockchainType = blockchainType;
            Version = version;
            Sequence = sequence;
            Configuration = configuration;
            ExpectedBlockNumber = expectedBlockNumber;
            Mode = mode;
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
                expectedBlockNumber: configuration.StartBlock,
                CrawlerMode.Indexing);
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
            return ExpectedBlockNumber == blockNumber;
        }

        public bool HaveToWaitFor(ChainHead chainHead, CrawlingDirection direction)
        {
            if (!chainHead.BlockNumber.HasValue)
            {
                return false;
            }

            if (!Configuration.IsInfinite)
            {
                return false;
            }

            switch (direction)
            {
                case CrawlingDirection.Forward:
                    return chainHead.IsCatchCrawlerUp && IsOnBlock(chainHead.BlockNumber.Value);
                
                case CrawlingDirection.Backward:
                    return chainHead.IsCatchCrawlerUp;

                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, string.Empty);
            }
        }

        public void StartWaitingForChainHead()
        {
            if (Mode != CrawlerMode.Indexing)
            {
                throw new InvalidOperationException($"Waiting for chain head can be started only in {CrawlerMode.Indexing} mode");
            }

            Mode = CrawlerMode.WaitingForChainHead;
        }

        public void StopWaitingForChainHead()
        {
            if (Mode != CrawlerMode.WaitingForChainHead)
            {
                throw new InvalidOperationException($"Waiting for chain head can be stopped only in {CrawlerMode.WaitingForChainHead} mode");
            }

            Mode = CrawlerMode.Indexing;
        }
    }
}
