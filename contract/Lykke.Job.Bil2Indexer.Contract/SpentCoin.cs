using System.Runtime.Serialization;
using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.Contract
{
    [PublicAPI]
    [DataContract]
    public class SpentCoin
    {
        [DataMember(Order = 0)]
        public CoinId Id { get; }

        [DataMember(Order = 1)]
        public UMoney Value { get; }

        [CanBeNull]
        [DataMember(Order = 2)]
        public AddressTag Tag { get; }

        [CanBeNull]
        [DataMember(Order = 3)]
        public AddressTagType? TagType { get; }
        
        [CanBeNull]
        [DataMember(Order = 4)]
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
