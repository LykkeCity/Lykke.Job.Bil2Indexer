using JetBrains.Annotations;

namespace Lykke.Job.Bil2Indexer.Workflow.Events
{
    public class BlockAssembledEvent
    {
        public string BlockchainType { get; set; }
        public string BlockId { get; set; }

        public override string ToString()
        {
            return $"{BlockchainType}:{BlockId}";
        }
    }
}
