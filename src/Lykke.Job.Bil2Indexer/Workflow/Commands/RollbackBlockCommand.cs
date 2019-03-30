using Lykke.Job.Bil2Indexer.Domain;

namespace Lykke.Job.Bil2Indexer.Workflow.Commands
{
    public class RollbackBlockCommand
    {
        public string BlockchainType { get; set; }
        public string BlockId { get; set; }
        public long BlockNumber { get; set; }
        public string PreviousBlockId { get; set; }

        public override string ToString()
        {
            return $"{BlockchainType}:{BlockId}";
        }
    }
}
