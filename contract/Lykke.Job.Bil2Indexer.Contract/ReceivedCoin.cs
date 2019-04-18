using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.Contract
{
    [PublicAPI]
    public class ReceivedCoin
    {
        public int Number { get; }

        public UMoney Value { get; }

        [CanBeNull]
        public AddressTag Tag { get; }

        [CanBeNull]
        public AddressTagType? TagType { get; }
        
        [CanBeNull]
        public long? Nonce { get; }

        public ReceivedCoin(
            int number,
            UMoney value,
            AddressTag tag,
            AddressTagType? tagType,
            long? nonce)
        {
            Number = number;
            Value = value;
            Tag = tag;
            TagType = tagType;
            Nonce = nonce;
        }

        public override string ToString()
        {
            return $"{Number} = {Value}";
        }
    }
}
