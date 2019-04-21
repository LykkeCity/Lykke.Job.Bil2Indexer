using System.Runtime.Serialization;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models.Props
{
    public enum TransactionBroadcastingErrorEntity
    {
        [EnumMember(Value = "notEnoughBalance")] NotEnoughBalance,
        [EnumMember(Value = "feeTooLow")] FeeTooLow,
        [EnumMember(Value = "rebuildRequired")] RebuildRequired,
        [EnumMember(Value = "transientFailure")] TransientFailure,
        [EnumMember(Value = "retryLater")] RetryLater,
    }
}
