using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Lykke.Job.Bil2Indexer.Contract.Events
{
    [PublicAPI]
    [DataContract]
    public class BlockRolledBackEvent
    {
        [DataMember(Order = 0)]
        public string BlockchainType { get; set; }
        
        [DataMember(Order = 1)]
        public long BlockNumber { get; set; }
        
        [DataMember(Order = 2)]
        public string BlockId { get; set; }
        
        [DataMember(Order = 3)]
        public string PreviousBlockId { get; set; }

        public override string ToString()
        {
            return $"{BlockchainType}:{BlockNumber}:{BlockId}";
        }
    }
}
