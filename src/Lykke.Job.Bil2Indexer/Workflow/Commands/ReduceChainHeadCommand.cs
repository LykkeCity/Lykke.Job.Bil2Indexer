using System.Runtime.Serialization;
using Lykke.Bil2.SharedDomain;

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
        public BlockId OutdatedBlockId { get; set; }

        [DataMember(Order = 3)]
        public long OutdatedBlockNumber { get; set; }

        [DataMember(Order = 4)]
        public BlockId TriggeredByBlockId { get; set; }
        
        public override string ToString()
        {
            return $"{BlockchainType}:{ToBlockNumber}:{OutdatedBlockId}";
        }
    }
}
