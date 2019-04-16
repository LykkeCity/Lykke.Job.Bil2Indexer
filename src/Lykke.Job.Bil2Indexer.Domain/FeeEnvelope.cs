using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class FeeEnvelope
    {
        public string BlockchainType { get; }
        public BlockId BlockId { get; }
        public TransactionId TransactionId { get; }
        public Fee Fee { get; }

        public FeeEnvelope(
            string blockchainType,
            BlockId blockId,
            TransactionId transactionId,
            Fee fee)
        {
            BlockchainType = blockchainType;
            BlockId = blockId;
            TransactionId = transactionId;
            Fee = fee;
        }

        public override string ToString()
        {
            return $"{BlockchainType}:{BlockId}:{TransactionId}:{Fee}";
        }
    }
}
