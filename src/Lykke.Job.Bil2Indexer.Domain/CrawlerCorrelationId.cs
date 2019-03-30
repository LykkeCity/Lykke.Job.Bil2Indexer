using System;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class CrawlerCorrelationId : IEquatable<CrawlerCorrelationId>
    {
        public CrawlerConfiguration Configuration { get; }
        public long Sequence { get; }

        public CrawlerCorrelationId(CrawlerConfiguration configuration, long sequence)
        {
            Configuration = configuration;
            Sequence = sequence;
        }

        public static CrawlerCorrelationId Parse(string correlationIdString)
        {
            if (correlationIdString == null)
            {
                throw new ArgumentNullException(nameof(correlationIdString));
            }

            var lastColonIndex = correlationIdString.LastIndexOf(":", StringComparison.InvariantCultureIgnoreCase);
            var configurationString = correlationIdString.Substring(0, lastColonIndex);
            var sequenceString = correlationIdString.Substring(lastColonIndex + 1);
            var sequence = long.Parse(sequenceString);

            var configuration = CrawlerConfiguration.Parse(configurationString);

            return new CrawlerCorrelationId(configuration, sequence);
        }

        public override string ToString()
        {
            return $"{Configuration}:{Sequence}";
        }

        public bool IsPreviousOf(CrawlerCorrelationId another)
        {
            if (!Configuration.Equals(another.Configuration))
            {
                throw new InvalidOperationException($"Configurations mismatch: {Configuration} another: {another}");
            }

            return Sequence + 1 == another.Sequence;
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
            return Equals(Configuration, other.Configuration) && Sequence == other.Sequence;
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

            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((CrawlerCorrelationId) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Configuration != null ? Configuration.GetHashCode() : 0) * 397) ^ Sequence.GetHashCode();
            }
        }
    }
}
