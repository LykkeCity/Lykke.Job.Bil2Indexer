namespace Lykke.Job.Bil2Indexer.Workflow.Events
{
    public class CrawlerMovedEvent
    {
        public string BlockchainType { get; set; }
        public long BlockNumber { get; set; }
    }
}
