using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.Contract.Events
{
    [PublicAPI]
    public class SpentCoin
    {
        public CoinId Id { get; }

        public UMoney Value { get; }

        [CanBeNull]
        public Address Address { get; }

        [CanBeNull]
        public AddressTag Tag { get; }

        [CanBeNull]
        public AddressTagType? TagType { get; }
        
        [CanBeNull]
        public long? Nonce { get; }

        public SpentCoin(
            CoinId id,
            UMoney value,
            Address address,
            AddressTag tag,
            AddressTagType? tagType,
            long? nonce)
        {
            Id = id;
            Value = value;
            Address = address;
            Tag = tag;
            TagType = tagType;
            Nonce = nonce;
        }

        public override string ToString()
        {
            return $"{Id}:{Address} = {Value}";
        }
    }
}
