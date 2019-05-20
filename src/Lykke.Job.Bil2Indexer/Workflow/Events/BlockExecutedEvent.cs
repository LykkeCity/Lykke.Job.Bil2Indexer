using System.Runtime.Serialization;
using Lykke.Job.Bil2Indexer.Workflow.Commands;

namespace Lykke.Job.Bil2Indexer.Workflow.Events
{
    [DataContract]
    public class BlockExecutedEvent
    {
        [DataMember(Order = 0)]
        public string BlockchainType { get; set; }
        
        [DataMember(Order = 1)]
        public string BlockId { get; set; }
        
        [DataMember(Order = 2)]
        public long BlockNumber { get; set; }

        [DataMember(Order = 3)]
        public BlockExecutionTrigger TriggeredBy { get; set; }

        public override string ToString()
        {
            return $"{BlockchainType}:{BlockId}:({BlockNumber}:{TriggeredBy}";
        }
    }
}
