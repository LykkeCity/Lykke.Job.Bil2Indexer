using System.Runtime.Serialization;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;

namespace Lykke.Job.Bil2Indexer.Workflow.Commands
{
    [DataContract]
    public class WaitForChainHeadCommand
    {
        [DataMember(Order = 0)]
        public string BlockchainType { get; set; }

        [DataMember(Order = 1)]
        public CrawlingDirection Direction { get; set; }
        
        [DataMember(Order = 2)]
        public long TargetBlockNumber { get; set; }
        
        [DataMember(Order = 3)]
        public BlockId OutdatedBlockId { get; set; }
        
        [DataMember(Order = 4)]
        public long OutdatedBlockNumber { get; set; }
        
        [DataMember(Order = 5)]
        public BlockId TriggeredByBlockId { get; set; }
    }
}
