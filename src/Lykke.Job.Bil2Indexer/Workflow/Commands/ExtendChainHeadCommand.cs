using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Workflow.Commands
{
    public class ExtendChainHeadCommand
    {
        public string BlockchainType { get; set; }
        public long ToBlockNumber { get; set; }
        public BlockId ToBlockId { get; set; }
        public long ChainHeadVersion { get; set; }

        public override string ToString()
        {
            return $"{BlockchainType}:{ToBlockNumber}({ChainHeadVersion})";
        }
    }
}
