namespace Lykke.Job.Bil2Indexer.Workflow.Events
{
    public class ChainHeadExtendedEvent
    {
        public string BlockchainType { get; set; }
        public long ChainHeadSequence { get; set; }
        public string BlockId { get; set; }
        public long BlockNumber { get; set; }

        public override string ToString()
        {
            return $"{BlockchainType}({ChainHeadSequence}):{BlockNumber}:{BlockId}";
        }
    }
}
