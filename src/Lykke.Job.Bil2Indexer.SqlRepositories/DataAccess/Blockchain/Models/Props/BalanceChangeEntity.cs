using System.Runtime.Serialization;
using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models.Props
{
    [DataContract]
    internal class BalanceChangeEntity
    {
        [DataMember(Order = 0)]
        public string TransferId { get; set; }

        [DataMember(Order = 1)]
        public Asset Asset { get; set; }

        [DataMember(Order = 2)]
        public Money Value { get; set; }

        [DataMember(Order = 3)]
        public Address Address { get; set; }

        [CanBeNull, DataMember(Order = 4)]
        public AddressTag Tag { get; set; }

        [CanBeNull, DataMember(Order = 5)]
        public AddressTagType? TagType { get; set; }

        [CanBeNull, DataMember(Order = 6)]
        public long? Nonce { get; set; }
    }
}
