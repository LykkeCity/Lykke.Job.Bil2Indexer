using System;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public sealed class ChainHeadCorrelationId : IEquatable<ChainHeadCorrelationId>
    {
        public const string Type = "ch#";

        public string BlockchainType { get; }
        public long ModeSequence { get; }
        public long BlockSequence { get; }
        public long CrawlerSequence { get; }

        public ChainHeadCorrelationId(string blockchainType, long modeSequence, long blockSequence, long crawlerSequence)
        {
            BlockchainType = blockchainType ?? throw new ArgumentNullException(nameof(blockchainType));
            ModeSequence = modeSequence;
            BlockSequence = blockSequence;
            CrawlerSequence = crawlerSequence;
        }

        public static ChainHeadCorrelationId Parse(string correlationIdString)
        {
            if (correlationIdString == null)
            {
                throw new ArgumentNullException(nameof(correlationIdString));
            }

            var type = CorrelationIdType.Parse(correlationIdString);

            if (type != Type)
            {
                throw new InvalidOperationException($"Invalid correlation id type: {type}");
            }

            var firstColonIndex = correlationIdString.IndexOf(':');
            var secondColonIndex = correlationIdString.IndexOf(':', firstColonIndex + 1);
            var thirdColonIndex = correlationIdString.IndexOf(':', secondColonIndex + 1);

            var blockchainType = correlationIdString.Substring(3, firstColonIndex - 3);
            var modeSequenceString = correlationIdString.Substring(firstColonIndex + 1, secondColonIndex - firstColonIndex - 1);
            var blockSequenceString = correlationIdString.Substring(secondColonIndex + 1, thirdColonIndex - secondColonIndex - 1);
            var crawlerSequenceString = correlationIdString.Substring(thirdColonIndex + 1);

            var modeSequence = long.Parse(modeSequenceString);
            var blockSequence = long.Parse(blockSequenceString);
            var crawlerSequence = long.Parse(crawlerSequenceString);

            return new ChainHeadCorrelationId(blockchainType, modeSequence, blockSequence, crawlerSequence);
        }

        public override string ToString()
        {
            return $"{Type}{BlockchainType}:{ModeSequence}:{BlockSequence}:{CrawlerSequence}";
        }

        public bool IsPreviousOf(ChainHeadCorrelationId another, ChainHeadMode mode)
        {
            if (!BlockchainType.Equals(another.BlockchainType))
            {
                throw new InvalidOperationException($"Blockchain type mismatch: {BlockchainType}, another: {another}");
            }

            // AttachToCrawler, DetachFromCrawler
            if (ModeSequence + 1 == another.ModeSequence && BlockSequence == another.BlockSequence && CrawlerSequence == another.CrawlerSequence)
            {
                return true;
            }

            switch (mode)
            {
                case ChainHeadMode.CatchesCrawlerUp:
                {
                    // ExecuteTransferCoinsBlock, ExtendTo
                    if (ModeSequence == another.ModeSequence && BlockSequence + 1 == another.BlockSequence)
                    {
                        return true;
                    }

                    break;
                }

                case ChainHeadMode.FollowsCrawler:
                {
                    // ExecuteTransferCoinsBlock, ExtendTo, ReduceTo
                    if (ModeSequence == another.ModeSequence && CrawlerSequence + 1 == another.CrawlerSequence)
                    {
                        return true;
                    }

                    break;
                }

                default:
                    throw new InvalidOperationException($"Unknown chain head mode {mode}");
            }

            return false;
        }

        public bool IsLegacyRelativeTo(ChainHeadCorrelationId another, ChainHeadMode mode)
        {
            if (!BlockchainType.Equals(another.BlockchainType))
            {
                throw new InvalidOperationException($"Blockchain type mismatch: {BlockchainType}, another: {another}");
            }

            // AttachToCrawler, DetachFromCrawler
            if (ModeSequence < another.ModeSequence)
            {
                return true;
            }

            switch (mode)
            {
                case ChainHeadMode.CatchesCrawlerUp:
                {
                    // ExecuteTransferCoinsBlock, ExtendTo
                    if (ModeSequence == another.ModeSequence && BlockSequence < another.BlockSequence)
                    {
                        return true;
                    }

                    break;
                }

                case ChainHeadMode.FollowsCrawler:
                {
                    // ExecuteTransferCoinsBlock, ExtendTo, ReduceTo
                    if (ModeSequence == another.ModeSequence && CrawlerSequence < another.CrawlerSequence)
                    {
                        return true;
                    }

                    break;
                }

                default:
                    throw new InvalidOperationException($"Unknown chain head mode {mode}");
            }

            return false;
        }

        public bool IsPrematureRelativeTo(ChainHeadCorrelationId another, ChainHeadMode mode)
        {
            if (!BlockchainType.Equals(another.BlockchainType))
            {
                throw new InvalidOperationException($"Blockchain type mismatch: {BlockchainType}, another: {another}");
            }
            
            // AttachToCrawler, DetachFromCrawler
            if (ModeSequence > another.ModeSequence)
            {
                return true;
            }

            switch (mode)
            {
                case ChainHeadMode.CatchesCrawlerUp:
                {
                    // ExecuteTransferCoinsBlock, ExtendTo
                    if (ModeSequence == another.ModeSequence && BlockSequence > another.BlockSequence)
                    {
                        return true;
                    }

                    break;
                }

                case ChainHeadMode.FollowsCrawler:
                {
                    // ExecuteTransferCoinsBlock, ExtendTo, ReduceTo
                    if (ModeSequence == another.ModeSequence && CrawlerSequence > another.CrawlerSequence)
                    {
                        return true;
                    }

                    break;
                }

                default:
                    throw new InvalidOperationException($"Unknown chain head mode {mode}");
            }

            return false;
        }

        public bool Equals(ChainHeadCorrelationId other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(BlockchainType, other.BlockchainType) && 
                   ModeSequence == other.ModeSequence && 
                   BlockSequence == other.BlockSequence && 
                   CrawlerSequence == other.CrawlerSequence;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is ChainHeadCorrelationId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = BlockchainType.GetHashCode();
                hashCode = (hashCode * 397) ^ ModeSequence.GetHashCode();
                hashCode = (hashCode * 397) ^ BlockSequence.GetHashCode();
                hashCode = (hashCode * 397) ^ CrawlerSequence.GetHashCode();
                return hashCode;
            }
        }
    }
}
