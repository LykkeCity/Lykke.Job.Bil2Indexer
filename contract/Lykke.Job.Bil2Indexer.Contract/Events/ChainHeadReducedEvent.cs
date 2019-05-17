using System.Runtime.Serialization;

namespace Lykke.Job.Bil2Indexer.Contract.Events
{
    [DataContract]
    public class ChainHeadReducedEvent
    {
        [DataMember(Order = 0)]
        public string BlockchainType { get; set; }
        
        [DataMember(Order = 1)]
        public long ChainHeadSequence { get; set; }
        
        [DataMember(Order = 2)]
        public string ToBlockId { get; set; }
        
        [DataMember(Order = 3)]
        public long ToBlockNumber { get; set; }
        
        [DataMember(Order = 4)]
        public string PreviousBlockId { get; set; }
        
        [DataMember(Order = 5)]
        public string OutdatedBlockId { get; set; }
        
        [DataMember(Order = 6)]
        public long OutdatedBlockNumber { get; set; }

        [DataMember(Order = 7)]
        public object TriggeredByBlockId { get; set; }

        public override string ToString()
        {
            return $"{BlockchainType}({ChainHeadSequence}):{ToBlockNumber}:{ToBlockId}";
        }
    }
}
