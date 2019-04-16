﻿using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.Contract.Events
{
    [PublicAPI]
    public class ReceivedCoin
    {
        public int Number { get; }

        public UMoney Value { get; }

        [CanBeNull]
        public Address Address { get; }

        [CanBeNull]
        public AddressTag Tag { get; }

        [CanBeNull]
        public AddressTagType? TagType { get; }
        
        [CanBeNull]
        public long? Nonce { get; }

        public ReceivedCoin(
            int number,
            UMoney value,
            Address address,
            AddressTag tag,
            AddressTagType? tagType,
            long? nonce)
        {
            Number = number;
            Value = value;
            Address = address;
            Tag = tag;
            TagType = tagType;
            Nonce = nonce;
        }

        public override string ToString()
        {
            return $"{Number}:{Address} = {Value}";
        }
    }
}
