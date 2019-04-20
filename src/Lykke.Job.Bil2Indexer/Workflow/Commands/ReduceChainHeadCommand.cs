using System.Runtime.Serialization;

namespace Lykke.Job.Bil2Indexer.Workflow.Commands
{
    [DataContract]
    public class ReduceChainHeadCommand
    {
        [DataMember(Order = 0)]
        public string BlockchainType { get; set; }
        
        [DataMember(Order = 1)]
        public long ToBlockNumber { get; set; }
        
        [DataMember(Order = 2)]
        public string ToBlockId { get; set; }
        
        [DataMember(Order = 3)]
        public string BlockIdToRollback { get; set; }
        
        [DataMember(Order = 4)]
        public long ChainHeadVersion { get; set; }

        public override string ToString()
        {
            return $"{BlockchainType}:{ToBlockNumber}({ChainHeadVersion})";
        }
    }
}
