using System;
using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.Contract.Events
{
    [PublicAPI]
    public class Transfer
    {
        /// <summary>
        /// ID of the transfer within the transaction.
        /// Can group several balance changing operations into the single transfer,
        /// or can be just the output number.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Value for which the balance of the address was changed.
        /// Can be positive to increase the balance or negative to decrease the balance.
        /// </summary>
        public Money Value { get; }

        /// <summary>
        /// Optional.
        /// Address.
        /// </summary>
        [CanBeNull]
        public Address Address { get; }

        /// <summary>
        /// Optional.
        /// Tag of the address.
        /// </summary>
        [CanBeNull]
        public AddressTag Tag { get; }

        /// <summary>
        /// Optional.
        /// Type of the address tag.
        /// </summary>
        [CanBeNull]
        public AddressTagType? TagType { get; }

        /// <summary>
        /// Optional.
        /// Nonce number of the transaction for the address.
        /// </summary>
        [CanBeNull]
        public long? Nonce { get; }

        public Transfer(
            string id, 
            Money value, 
            Address address = null, 
            AddressTag tag = null, 
            AddressTagType? tagType = null,
            long? nonce = null)
        {           
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Should be not empty string", nameof(id));

            if (tag != null && address == null)
                throw new ArgumentException("If the tag is specified, the address should be specified too");

            if (tagType.HasValue && tag == null)
                throw new ArgumentException("If the tag type is specified, the tag should be specified too");

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
