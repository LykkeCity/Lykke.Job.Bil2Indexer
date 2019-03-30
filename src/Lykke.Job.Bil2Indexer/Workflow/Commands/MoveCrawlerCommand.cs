namespace Lykke.Job.Bil2Indexer.Workflow.Commands
{
    public class MoveCrawlerCommand
    {
        public string BlockchainType { get; set; }
        public long NextBlockNumber { get; set; }

        public override string ToString()
        {
            return $"{BlockchainType}:{NextBlockNumber}";
        }
    }
}
