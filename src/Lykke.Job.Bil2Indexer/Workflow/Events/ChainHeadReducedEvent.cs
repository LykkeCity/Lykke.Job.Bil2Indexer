namespace Lykke.Job.Bil2Indexer.Workflow.Events
{
    public class ChainHeadReducedEvent
    {
        public string BlockchainType { get; set; }
        public long ChainHeadSequence { get; set; }
        public string ToBlockId { get; set; }
        public long ToBlockNumber { get; set; }
        public string PreviousBlockId { get; set; }
        public string BlockIdToRollback { get; set; }

        public override string ToString()
        {
            return $"{BlockchainType}({ChainHeadSequence}):{ToBlockNumber}:{ToBlockId}";
        }
    }
}
