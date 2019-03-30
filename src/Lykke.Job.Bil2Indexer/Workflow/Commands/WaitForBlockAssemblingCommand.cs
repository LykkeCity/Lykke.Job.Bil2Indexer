namespace Lykke.Job.Bil2Indexer.Workflow.Commands
{
    public class WaitForBlockAssemblingCommand
    {
        public string BlockchainType { get; set; }
        public string BlockId { get; set; }
    }
}
