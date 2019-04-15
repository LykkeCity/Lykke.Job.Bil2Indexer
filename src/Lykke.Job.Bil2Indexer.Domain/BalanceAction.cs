using Lykke.Bil2.SharedDomain;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class BalanceAction
    {
        public Address Address { get; }
        public Asset Asset { get; }
        public Money Amount { get; }
        public long BlockNumber { get; }
        public string BlockId { get; }
        public string TransactionId { get; }

        public BalanceAction(Address address, Asset asset, Money amount, long blockNumber, string blockId, string transactionId)
        {
            Address = address;
            Asset = asset;
            Amount = amount;
            BlockNumber = blockNumber;
            BlockId = blockId;
            TransactionId = transactionId;
        }
    }
}
