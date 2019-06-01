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
        public long ModeSequence { get; private set; }
        public long BlockSequence { get; private set; }
        public long CrawlerSequence { get; private set; }
        public ChainHeadMode Mode { get; private set; }

        public bool IsFollowCrawler => Mode == ChainHeadMode.FollowsCrawler;
        public bool IsCatchCrawlerUp => Mode == ChainHeadMode.CatchesCrawlerUp;

        public ChainHead(
            string blockchainType,
            long firstBlockNumber,
            long version,
            long modeSequence,
            long blockSequence,
            long crawlerSequence,
            long? blockNumber,
            BlockId blockId,
            BlockId previousBlockId,
            ChainHeadMode mode)
        {
            BlockchainType = blockchainType;
            FirstBlockNumber = firstBlockNumber;
            Version = version;
            ModeSequence = modeSequence;
            BlockSequence = blockSequence;
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
                modeSequence: 0,
                blockSequence: 0,
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
                case ChainHeadMode.CatchesCrawlerUp:
                    ++BlockSequence;
                    break;

                case ChainHeadMode.FollowsCrawler:
                    ++CrawlerSequence;
                    ++BlockSequence;

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(Mode), Mode, string.Empty);
            }
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
            ++BlockSequence;
        }

        public bool CanExtendTo(long blockNumber)
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

        public bool CanReduceTo(long blockNumber)
        {
            if (BlockNumber.HasValue && BlockNumber == blockNumber + 1 && blockNumber >= FirstBlockNumber)
            {
                return true;
            }

            return false;
        }

        public void AttachToCrawler(long crawlerSequence)
        {
            if (IsCatchCrawlerUp)
            {
                throw new InvalidOperationException($"Chain head can be attached to the crawler only in {ChainHeadMode.CatchesCrawlerUp} mode");
            }
            
            Mode = ChainHeadMode.FollowsCrawler;
            CrawlerSequence = crawlerSequence;

            ++ModeSequence;
        }

        public void DetachFromCrawler()
        {
            if (IsFollowCrawler)
            {
                throw new InvalidOperationException($"Chain head can be detached from the crawler only in {ChainHeadMode.FollowsCrawler} mode");
            }

            Mode = ChainHeadMode.CatchesCrawlerUp;

            ++ModeSequence;
        }

        public bool CanAttachToCrawler(long messageModeSequence)
        {
            return IsCatchCrawlerUp && ModeSequence == messageModeSequence;
        }

        public bool CanDetachFromCrawler(long messageModeSequence)
        {
            return IsFollowCrawler && ModeSequence == messageModeSequence;
        }

        public bool HaveToAttachTo(Crawler infiniteCrawler)
        {
            return IsCatchCrawlerUp && infiniteCrawler.IsWaitingForChainHead;
        }

        public bool HaveToDetachFrom(Crawler infiniteCrawler)
        {
            // In case when distance between crawler and chain head is too large
            // a lot of premature messages can be collected in the queue, making harder
            // and harder to find correct message in the queue. In this case
            // better to detach chain head from the crawler and let it catch up the crawler
            // again. All en-queued messages which are stick to the crawler will be
            // treated as obsolete after this.
            return IsFollowCrawler && infiniteCrawler.ExpectedBlockNumber - BlockNumber > 100;
        }

        public ChainHeadCorrelationId GetCorrelationId()
        {
            return new ChainHeadCorrelationId(BlockchainType, ModeSequence, BlockSequence, CrawlerSequence);
        }

        public ChainHeadCorrelationId GetCorrelationId(CrawlerCorrelationId crawlerCorrelationId)
        {
            return new ChainHeadCorrelationId(BlockchainType, ModeSequence, BlockSequence, crawlerCorrelationId.Sequence);
        }

        public override string ToString()
        {
            return $"{BlockchainType}:{BlockNumber}({Version})";
        }
    }
}
