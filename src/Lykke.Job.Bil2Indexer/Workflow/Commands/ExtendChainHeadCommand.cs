using System.Runtime.Serialization;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Workflow.Commands
{
    [DataContract]
    public class ExtendChainHeadCommand
    {
        [DataMember(Order = 0)]
        public string BlockchainType { get; set; }
        
        [DataMember(Order = 1)]
        public long ToBlockNumber { get; set; }
        
        [DataMember(Order = 2)]
        public BlockId ToBlockId { get; set; }
        
        [DataMember(Order = 3)]
        public long ChainHeadVersion { get; set; }

        public override string ToString()
        {
            return $"{BlockchainType}:{ToBlockNumber}({ChainHeadVersion})";
        }
    }
}
