using System.Runtime.Serialization;

namespace Lykke.Job.Bil2Indexer.Workflow.Events
{
    [DataContract]
    public class BlockPartiallyExecutedEvent
    {
        [DataMember(Order = 0)]
        public string BlockchainType { get; set; }
        
        [DataMember(Order = 1)]
        public string BlockId { get; set; }
        
        [DataMember(Order = 2)]
        public long BlockNumber { get; set; }

        public override string ToString()
        {
            return $"{BlockchainType}:{BlockId}:({BlockNumber}";
        }
    }
}
