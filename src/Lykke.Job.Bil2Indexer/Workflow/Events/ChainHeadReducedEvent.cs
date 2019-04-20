using System.Runtime.Serialization;

namespace Lykke.Job.Bil2Indexer.Workflow.Events
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
        public string BlockIdToRollback { get; set; }

        public override string ToString()
        {
            return $"{BlockchainType}({ChainHeadSequence}):{ToBlockNumber}:{ToBlockId}";
        }
    }
}
