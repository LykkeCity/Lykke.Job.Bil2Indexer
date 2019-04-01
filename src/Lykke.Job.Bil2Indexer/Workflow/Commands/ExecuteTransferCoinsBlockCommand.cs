namespace Lykke.Job.Bil2Indexer.Workflow.Commands
{
    public class ExecuteTransferCoinsBlockCommand
    {
        public string BlockchainType { get; set; }
        public string BlockId { get; set; }
        public long BlockVersion { get; set; }

        public override string ToString()
        {
            return $"{BlockchainType}:{BlockId}:{BlockVersion}";
        }
    }
}
