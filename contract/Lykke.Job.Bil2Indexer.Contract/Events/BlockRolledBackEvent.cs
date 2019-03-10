using JetBrains.Annotations;

namespace Lykke.Job.Bil2Indexer.Contract.Events
{
    [PublicAPI]
    public class BlockRolledBackEvent
    {
        public string BlockchainType { get; set; }
        public long BlockNumber { get; set; }
        public string BlockHash { get; set; }
        public string PreviousBlockHash { get; set; }

        public override string ToString()
        {
            return BlockHash;
        }
    }
}
