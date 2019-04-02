namespace Lykke.Job.Bil2Indexer.Workflow.Commands
{
    public class ExtendChainHeadCommand
    {
        public string BlockchainType { get; set; }
        public long NextBlockNumber { get; set; }
        public string NextBlockId { get; set; }
        public long ChainHeadVersion { get; set; }
        
        public override string ToString()
        {
            return $"{BlockchainType}:{NextBlockNumber}({ChainHeadVersion})";
        }
    }
}
