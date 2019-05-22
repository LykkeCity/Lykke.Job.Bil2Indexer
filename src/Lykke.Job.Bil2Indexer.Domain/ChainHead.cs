using System;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class ChainHead
    {
        public string BlockchainType { get; }
        public long Version { get; }
        public long FirstBlockNumber { get; }
        public long? BlockNumber { get; private set; }
        public BlockId BlockId { get; private set; }
        public BlockId PreviousBlockId { get; private set; }
        public long Sequence { get; private set; }
        public long CrawlerSequence { get; private set; }
        public ChainHeadMode Mode { get; private set; }

        public bool IsFollowCrawler => Mode == ChainHeadMode.FollowsCrawler;
        public bool IsCatchCrawlerUp => Mode == ChainHeadMode.CatchesCrawlerUp;

        public ChainHead(
            string blockchainType,
            long firstBlockNumber,
            long version,
            long sequence,
            long crawlerSequence,
            long? blockNumber,
            BlockId blockId,
            BlockId previousBlockId,
            ChainHeadMode mode)
        {
            BlockchainType = blockchainType;
            FirstBlockNumber = firstBlockNumber;
            Version = version;
            Sequence = sequence;
            CrawlerSequence = crawlerSequence;
            BlockNumber = blockNumber;
            BlockId = blockId;
            PreviousBlockId = previousBlockId;
            Mode = mode;
        }

        public static ChainHead CreateNew(
            string blockchainType,
            long firstBlockNumber)
        {
            return new ChainHead
            (
                blockchainType: blockchainType,
                firstBlockNumber: firstBlockNumber,
                version: 0,
                sequence: 0,
                crawlerSequence: 0,
                blockNumber: null,
                blockId: null,
                previousBlockId: null,
                mode: ChainHeadMode.CatchesCrawlerUp
            );
        }

        public void ExtendTo(long blockNumber, BlockId blockId, Crawler infiniteCrawler)
        {
            if (!CanExtendTo(blockNumber))
            {
                throw new InvalidOperationException($"Chain head can't be extended to the block {blockNumber}:{blockId}. Expected block number {BlockNumber + 1}. Current block is {BlockNumber}");
            }

            if (!infiniteCrawler.Configuration.IsInfinite)
            {
                throw new InvalidOperationException($"Crawler {infiniteCrawler} is not infinite");
            }

            BlockNumber = blockNumber;
            PreviousBlockId = BlockId;
            BlockId = blockId;

            switch (Mode)
            {
                case ChainHeadMode.CatchesCrawlerUp when infiniteCrawler.IsOnBlock(blockNumber):
                    Mode = ChainHeadMode.FollowsCrawler;
                    CrawlerSequence = infiniteCrawler.Sequence;
                    break;

                case ChainHeadMode.FollowsCrawler:
                    ++CrawlerSequence;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(Mode), Mode, string.Empty);
            }

            ++Sequence;
        }

        public void ReduceTo(long blockNumber, BlockId blockId)
        {
            if (!CanReduceTo(blockNumber))
            {
                throw new InvalidOperationException($"Chain head can't be reduced to the block {blockNumber}:{blockId}. Expected block number {BlockNumber - 1}. Current block is {BlockNumber}. First block number {FirstBlockNumber}");
            }

            if (Mode != ChainHeadMode.FollowsCrawler)
            {
                throw new InvalidOperationException($"Chain head reduction is possible only in {ChainHeadMode.FollowsCrawler} mode. Actual mode {Mode}");
            }

            BlockNumber = blockNumber;
            PreviousBlockId = BlockId;
            BlockId = blockId;

            ++CrawlerSequence;
            ++Sequence;
        }

        public ChainHeadCorrelationId GetCorrelationId()
        {
            return new ChainHeadCorrelationId(BlockchainType, Mode, Sequence, CrawlerSequence);
        }

        public ChainHeadCorrelationId GetCorrelationId(CrawlerCorrelationId crawlerCorrelationId)
        {
            return new ChainHeadCorrelationId(BlockchainType, Mode, Sequence, crawlerCorrelationId.Sequence);
        }

        public override string ToString()
        {
            return $"{BlockchainType}:{BlockNumber}({Version})";
        }

        private bool CanExtendTo(long blockNumber)
        {
            if (!BlockNumber.HasValue && FirstBlockNumber == blockNumber)
            {
                return true;
            }

            if (BlockNumber.HasValue && BlockNumber == blockNumber - 1)
            {
                return true;
            }

            return false;
        }

        private bool CanReduceTo(long blockNumber)
        {
            if (BlockNumber.HasValue && BlockNumber == blockNumber + 1 && blockNumber >= FirstBlockNumber)
            {
                return true;
            }

            return false;
        }
    }
}
