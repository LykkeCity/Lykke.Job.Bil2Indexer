using System.Runtime.Serialization;
using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models.Props
{
    [DataContract]
    internal class ReceivedCoinEntity
    {
        [DataMember(Order = 0)]
        public int CoinNumber { get; set; }

        [DataMember(Order = 1)]
        public Asset Asset { get; set; }
        
        [DataMember(Order = 2)]
        public UMoney Value { get; set; }

        [CanBeNull, DataMember(Order = 3)]
        public Address Address { get; set; }

        [CanBeNull, DataMember(Order = 4)]
        public AddressTag AddressTag { get; set; }

        [CanBeNull, DataMember(Order = 5)]
        public AddressTagType? AddressTagType { get; set; }

        [CanBeNull, DataMember(Order = 6)]
        public long? AddressNonce { get; set; }
    }
}
