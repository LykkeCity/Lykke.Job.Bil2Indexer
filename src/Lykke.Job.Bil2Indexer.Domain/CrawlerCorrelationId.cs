using System;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public sealed class CrawlerCorrelationId : IEquatable<CrawlerCorrelationId>
    {
        public const string Type = "cr#";

        public string BlockchainType { get; }
        public CrawlerConfiguration Configuration { get; }
        public long Sequence { get; }

        public CrawlerCorrelationId(string blockchainType, CrawlerConfiguration configuration, long sequence)
        {
            BlockchainType = blockchainType ?? throw new ArgumentNullException(nameof(blockchainType));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Sequence = sequence;
        }

        public static CrawlerCorrelationId Parse(string correlationIdString)
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
            var lastColonIndex = correlationIdString.LastIndexOf(':');
            var blockchainType = correlationIdString.Substring(3, firstColonIndex - 3);
            var configurationString = correlationIdString.Substring(firstColonIndex + 1, lastColonIndex - firstColonIndex - 1);
            var sequenceString = correlationIdString.Substring(lastColonIndex + 1);
            var sequence = long.Parse(sequenceString);

            var configuration = CrawlerConfiguration.Parse(configurationString);

            return new CrawlerCorrelationId(blockchainType, configuration, sequence);
        }

        public override string ToString()
        {
            return $"{Type}{BlockchainType}:{Configuration}:{Sequence}";
        }

        public bool IsPreviousOf(CrawlerCorrelationId another)
        {
            if (!BlockchainType.Equals(another.BlockchainType))
            {
                throw new InvalidOperationException($"Blockchain type mismatch: {BlockchainType} another: {another}");
            }

            if (!Configuration.Equals(another.Configuration))
            {
                throw new InvalidOperationException($"Configurations mismatch: {Configuration} another: {another}");
            }

            return Sequence + 1 == another.Sequence;
        }

        public bool IsLegacyRelativeTo(CrawlerCorrelationId another)
        {
            if (!BlockchainType.Equals(another.BlockchainType))
            {
                throw new InvalidOperationException($"Blockchain type mismatch: {BlockchainType} another: {another}");
            }

            if (!Configuration.Equals(another.Configuration))
            {
                throw new InvalidOperationException($"Configurations mismatch: {Configuration} another: {another}");
            }

            return Sequence < another.Sequence;
        }

        public bool IsPrematureRelativeTo(CrawlerCorrelationId another)
        {
            if (!BlockchainType.Equals(another.BlockchainType))
            {
                throw new InvalidOperationException($"Blockchain type mismatch: {BlockchainType} another: {another}");
            }

            if (!Configuration.Equals(another.Configuration))
            {
                throw new InvalidOperationException($"Configurations mismatch: {Configuration} another: {another}");
            }

            return Sequence > another.Sequence;
        }

        public bool IsTheSameAs(CrawlerCorrelationId another)
        {
            if (!BlockchainType.Equals(another.BlockchainType))
            {
                throw new InvalidOperationException($"Blockchain type mismatch: {BlockchainType} another: {another}");
            }

            if (!Configuration.Equals(another.Configuration))
            {
                throw new InvalidOperationException($"Configurations mismatch: {Configuration} another: {another}");
            }

            return Sequence == another.Sequence;
        }

        public bool Equals(CrawlerCorrelationId other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return BlockchainType.Equals(other.BlockchainType, StringComparison.InvariantCulture) && 
                   Configuration.Equals(other.Configuration) && 
                   Sequence == other.Sequence;
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

            return obj is CrawlerCorrelationId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = BlockchainType.GetHashCode();
                hashCode = (hashCode * 397) ^ Configuration.GetHashCode();
                hashCode = (hashCode * 397) ^ Sequence.GetHashCode();
                return hashCode;
            }
        }
    }
}
