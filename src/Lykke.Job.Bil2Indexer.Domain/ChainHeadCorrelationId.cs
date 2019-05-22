using System;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public sealed class ChainHeadCorrelationId : IEquatable<ChainHeadCorrelationId>
    {
        public const string Type = "ch#";

        public string BlockchainType { get; }
        public ChainHeadMode Mode { get; }
        public long Sequence { get; }
        public long CrawlerSequence { get; }

        public ChainHeadCorrelationId(string blockchainType, ChainHeadMode mode, long sequence, long crawlerSequence)
        {
            BlockchainType = blockchainType ?? throw new ArgumentNullException(nameof(blockchainType));
            Mode = mode;
            Sequence = sequence;
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
            var blockchainType = correlationIdString.Substring(3, firstColonIndex - 3);
            var secondColonIndex = correlationIdString.IndexOf(':', firstColonIndex + 1);
            var modeString = correlationIdString.Substring(firstColonIndex + 1, secondColonIndex - firstColonIndex - 1);
            var mode = (ChainHeadMode) int.Parse(modeString);
            long sequence;
            long crawlerSequence;

            switch (mode)
            {
                case ChainHeadMode.CatchesCrawlerUp:
                {
                    var sequenceString = correlationIdString.Substring(secondColonIndex + 1);
                    sequence = long.Parse(sequenceString);
                    crawlerSequence = 0;
                }
                    break;

                case ChainHeadMode.FollowsCrawler:
                {
                    var thirdColonIndex = correlationIdString.IndexOf(':', secondColonIndex + 1);
                    var sequenceString = correlationIdString.Substring(secondColonIndex + 1, thirdColonIndex - secondColonIndex -1);
                    sequence = long.Parse(sequenceString);
                    var crawlerSequenceString = correlationIdString.Substring(thirdColonIndex + 1);
                    crawlerSequence = long.Parse(crawlerSequenceString);
                }
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, string.Empty);
            }
            
            return new ChainHeadCorrelationId(blockchainType, mode, sequence, crawlerSequence);
        }

        public override string ToString()
        {
            switch (Mode)
            {
                case ChainHeadMode.CatchesCrawlerUp:
                    return $"{Type}{BlockchainType}:{(int)ChainHeadMode.CatchesCrawlerUp}:{Sequence}";
                
                case ChainHeadMode.FollowsCrawler:
                    return $"{Type}{BlockchainType}:{(int)ChainHeadMode.FollowsCrawler}:{Sequence}:{CrawlerSequence}";

                default:
                    throw new ArgumentOutOfRangeException(nameof(Mode), Mode, string.Empty);
            }
        }

        public bool IsPreviousOf(ChainHeadCorrelationId another)
        {
            if (!BlockchainType.Equals(another.BlockchainType))
            {
                throw new InvalidOperationException($"Blockchain type mismatch: {BlockchainType}, another: {another}");
            }

            switch (Mode)
            {
                case ChainHeadMode.CatchesCrawlerUp when another.Mode == ChainHeadMode.CatchesCrawlerUp:
                    return Sequence + 1 == another.Sequence;
                case ChainHeadMode.CatchesCrawlerUp when another.Mode == ChainHeadMode.FollowsCrawler:
                    return Sequence + 1 == another.Sequence;
                case ChainHeadMode.FollowsCrawler when another.Mode == ChainHeadMode.CatchesCrawlerUp:
                    return Sequence + 1 == another.Sequence;
                case ChainHeadMode.FollowsCrawler when another.Mode == ChainHeadMode.FollowsCrawler:
                    return CrawlerSequence + 1 == another.CrawlerSequence;
                default:
                    throw new InvalidOperationException($"Unknown chain mode: {Mode}, or another.Mode: {another.Mode}");
            }
        }

        public bool IsLegacyRelativeTo(ChainHeadCorrelationId another)
        {
            if (!BlockchainType.Equals(another.BlockchainType))
            {
                throw new InvalidOperationException($"Blockchain type mismatch: {BlockchainType}, another: {another}");
            }

            switch (Mode)
            {
                case ChainHeadMode.CatchesCrawlerUp when another.Mode == ChainHeadMode.CatchesCrawlerUp:
                    return Sequence < another.Sequence;
                case ChainHeadMode.CatchesCrawlerUp when another.Mode == ChainHeadMode.FollowsCrawler:
                    return Sequence < another.Sequence;
                case ChainHeadMode.FollowsCrawler when another.Mode == ChainHeadMode.CatchesCrawlerUp:
                    return Sequence < another.Sequence;
                case ChainHeadMode.FollowsCrawler when another.Mode == ChainHeadMode.FollowsCrawler:
                    return CrawlerSequence < another.CrawlerSequence;
                default:
                    throw new InvalidOperationException($"Unknown mode: {Mode}, or another.Mode: {another.Mode}");
            }
        }

        public bool IsPrematureRelativeTo(ChainHeadCorrelationId another)
        {
            if (!BlockchainType.Equals(another.BlockchainType))
            {
                throw new InvalidOperationException($"Blockchain type mismatch: {BlockchainType}, another: {another}");
            }

            switch (Mode)
            {
                case ChainHeadMode.CatchesCrawlerUp when another.Mode == ChainHeadMode.CatchesCrawlerUp:
                    return Sequence > another.Sequence;
                case ChainHeadMode.CatchesCrawlerUp when another.Mode == ChainHeadMode.FollowsCrawler:
                    return Sequence > another.Sequence;
                case ChainHeadMode.FollowsCrawler when another.Mode == ChainHeadMode.CatchesCrawlerUp:
                    return Sequence > another.Sequence;
                case ChainHeadMode.FollowsCrawler when another.Mode == ChainHeadMode.FollowsCrawler:
                    return CrawlerSequence > another.CrawlerSequence;
                default:
                    throw new InvalidOperationException($"Unknown mode: {Mode}, or another.Mode: {another.Mode}");
            }
        }

        public bool IsTheSameAs(ChainHeadCorrelationId another)
        {
            if (!BlockchainType.Equals(another.BlockchainType))
            {
                throw new InvalidOperationException($"Blockchain type mismatch: {BlockchainType}, another: {another}");
            }

            return Sequence == another.Sequence && Mode == another.Mode && CrawlerSequence == another.CrawlerSequence;
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
                   Mode == other.Mode && 
                   Sequence == other.Sequence && 
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
                hashCode = (hashCode * 397) ^ (int) Mode;
                hashCode = (hashCode * 397) ^ Sequence.GetHashCode();
                hashCode = (hashCode * 397) ^ CrawlerSequence.GetHashCode();
                return hashCode;
            }
        }
    }
}
