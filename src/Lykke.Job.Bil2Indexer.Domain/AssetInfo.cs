using System;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class AssetInfo : IEquatable<AssetInfo>
    {
        public string BlockchainType { get; }
        public Asset Asset { get; }
        public int Scale { get; }

        public AssetInfo(string blockchainType, Asset asset, int scale)
        {
            BlockchainType = blockchainType ?? throw new ArgumentNullException(nameof(blockchainType));
            Asset = asset ?? throw new ArgumentNullException(nameof(asset));
            Scale = scale;
        }

        public bool Equals(AssetInfo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(BlockchainType, other.BlockchainType) && Equals(Asset, other.Asset) && Scale == other.Scale;
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

            return Equals((AssetInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (BlockchainType != null ? BlockchainType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Asset != null ? Asset.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Scale;
                return hashCode;
            }
        }
    }
}
