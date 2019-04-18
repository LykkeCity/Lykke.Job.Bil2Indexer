using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.Contract
{
    [PublicAPI]
    public class SpentCoin
    {
        public CoinId Id { get; }

        public UMoney Value { get; }

        [CanBeNull]
        public AddressTag Tag { get; }

        [CanBeNull]
        public AddressTagType? TagType { get; }
        
        [CanBeNull]
        public long? Nonce { get; }

        public SpentCoin(
            CoinId id,
            UMoney value,
            AddressTag tag,
            AddressTagType? tagType,
            long? nonce)
        {
            Id = id;
            Value = value;
            Tag = tag;
            TagType = tagType;
            Nonce = nonce;
        }

        public override string ToString()
        {
            return $"{Id} = {Value}";
        }
    }
}
