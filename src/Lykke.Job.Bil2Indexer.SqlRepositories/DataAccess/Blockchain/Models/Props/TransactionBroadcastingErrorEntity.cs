using System.Runtime.Serialization;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models.Props
{
    internal enum TransactionBroadcastingErrorEntity
    {
        [EnumMember(Value = "notEnoughBalance")] NotEnoughBalance = 0,
        [EnumMember(Value = "feeTooLow")] FeeTooLow = 1,
        [EnumMember(Value = "rebuildRequired")] RebuildRequired = 2,
        [EnumMember(Value = "transientFailure")] TransientFailure = 3,
        [EnumMember(Value = "retryLater")] RetryLater = 4,
        [EnumMember(Value = "unknown")] Unknown = 5
    }
}
