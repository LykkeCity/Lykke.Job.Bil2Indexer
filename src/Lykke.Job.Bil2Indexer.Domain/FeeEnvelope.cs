using Lykke.Bil2.Contract.Common;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class FeeEnvelope
    {
        public string BlockchainType { get; }
        public string BlockId { get; }
        public string TransactionId { get; }
        public Fee Fee { get; }

        public FeeEnvelope(
            string blockchainType,
            string blockId,
            string transactionId,
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