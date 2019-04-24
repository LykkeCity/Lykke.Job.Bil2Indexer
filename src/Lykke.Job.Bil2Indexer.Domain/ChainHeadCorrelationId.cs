using System;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public sealed class ChainHeadCorrelationId : IEquatable<ChainHeadCorrelationId>
    {
        public string BlockchainType { get; }
        public long Sequence { get; }

        public ChainHeadCorrelationId(string blockchainType, long sequence)
        {
            BlockchainType = blockchainType ?? throw new ArgumentNullException(nameof(blockchainType));
            Sequence = sequence;
        }

        public static ChainHeadCorrelationId Parse(string correlationIdString)
        {
            if (correlationIdString == null)
            {
                throw new ArgumentNullException(nameof(correlationIdString));
            }

            var firstColonIndex = correlationIdString.IndexOf(':');
            var blockchainType = correlationIdString.Substring(0, firstColonIndex);
            var sequenceString = correlationIdString.Substring(firstColonIndex + 1);
            var sequence = long.Parse(sequenceString);

            return new ChainHeadCorrelationId(blockchainType, sequence);
        }

        public override string ToString()
        {
            return $"{BlockchainType}:{Sequence}";
        }

        public bool IsPreviousOf(ChainHeadCorrelationId another)
        {
            if (!BlockchainType.Equals(another.BlockchainType))
            {
                throw new InvalidOperationException($"Blockchain type mismatch: {BlockchainType} another: {another}");
            }

            return Sequence + 1 == another.Sequence;
        }

        public bool IsLegacyRelativeTo(ChainHeadCorrelationId another)
        {
            if (!BlockchainType.Equals(another.BlockchainType))
            {
                throw new InvalidOperationException($"Blockchain type mismatch: {BlockchainType} another: {another}");
            }

            return Sequence < another.Sequence;
        }

        public bool IsPrematureRelativeTo(ChainHeadCorrelationId another)
        {
            if (!BlockchainType.Equals(another.BlockchainType))
            {
                throw new InvalidOperationException($"Blockchain type mismatch: {BlockchainType} another: {another}");
            }

            return Sequence > another.Sequence;
        }

        public bool IsTheSameAs(ChainHeadCorrelationId another)
        {
            if (!BlockchainType.Equals(another.BlockchainType))
            {
                throw new InvalidOperationException($"Blockchain type mismatch: {BlockchainType} another: {another}");
            }

            return Sequence == another.Sequence;
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

            return BlockchainType.Equals(other.BlockchainType, StringComparison.InvariantCulture) && Sequence == other.Sequence;
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

            return Equals((ChainHeadCorrelationId) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return BlockchainType.GetHashCode() * 397 ^ Sequence.GetHashCode();
            }
        }
    }
}
