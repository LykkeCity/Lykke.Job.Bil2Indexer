using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Contract.Events;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class BalanceAction
    {
        public AccountId AccountId { get; }
        public Money Amount { get; }
        public long BlockNumber { get; }
        public BlockId BlockId { get; }
        public TransactionId TransactionId { get; }

        public BalanceAction(AccountId accountId, Money amount, long blockNumber, BlockId blockId, TransactionId transactionId)
        {
            AccountId = accountId;
            Amount = amount;
            BlockNumber = blockNumber;
            BlockId = blockId;
            TransactionId = transactionId;
        }
    }
}
