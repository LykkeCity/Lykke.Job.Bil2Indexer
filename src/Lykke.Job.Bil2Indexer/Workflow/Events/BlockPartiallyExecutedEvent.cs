namespace Lykke.Job.Bil2Indexer.Workflow.Events
{
    public class BlockPartiallyExecutedEvent
    {
        public string BlockchainType { get; set; }
        public string BlockId { get; set; }
        public long BlockNumber { get; set; }

        public override string ToString()
        {
            return $"{BlockchainType}:{BlockId}:({BlockNumber}";
        }
    }
}
