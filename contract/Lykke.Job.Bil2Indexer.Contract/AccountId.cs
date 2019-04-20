using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Contract
{
    [PublicAPI]
    [DataContract]
    public class AccountId : IEquatable<AccountId>
    {
        [DataMember(Order = 0)]
        public Address Address { get; }

        [DataMember(Order = 1)]
        public Asset Asset { get; }

        public AccountId(Address address, Asset asset)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
            Asset = asset ?? throw new ArgumentNullException(nameof(asset));
        }

        public override string ToString()
        {
            return $"{Address}:{Asset}";
        }

        public bool Equals(AccountId other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(Address, other.Address) && Equals(Asset, other.Asset);
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

            return Equals((AccountId) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Address != null ? Address.GetHashCode() : 0) * 397) ^ (Asset != null ? Asset.GetHashCode() : 0);
            }
        }
    }
}
