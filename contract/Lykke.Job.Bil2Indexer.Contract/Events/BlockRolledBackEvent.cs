using JetBrains.Annotations;

namespace Lykke.Job.Bil2Indexer.Contract.Events
{
    [PublicAPI]
    public class BlockRolledBackEvent
    {
        public string BlockchainType { get; set; }
        public long BlockNumber { get; set; }
        public string BlockId { get; set; }
        public string PreviousBlockId { get; set; }

        public override string ToString()
        {
            return BlockId;
        }
    }
}
