using System;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models.Props;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions.Mappers
{
    internal static class TransactionBroadcastingErrorMapper
    {
        public static TransactionBroadcastingErrorEntity ToDbEntity(this TransactionBroadcastingError source)
        {
            switch (source)
            {
                case TransactionBroadcastingError.FeeTooLow:
                    return TransactionBroadcastingErrorEntity.FeeTooLow;
                case TransactionBroadcastingError.NotEnoughBalance:
                    return TransactionBroadcastingErrorEntity.NotEnoughBalance;
                case TransactionBroadcastingError.RebuildRequired:
                    return TransactionBroadcastingErrorEntity.RebuildRequired;
                case TransactionBroadcastingError.RetryLater:
                    return TransactionBroadcastingErrorEntity.RetryLater;
                case TransactionBroadcastingError.TransientFailure:
                    return TransactionBroadcastingErrorEntity.TransientFailure;
                case TransactionBroadcastingError.Unknown:
                    return TransactionBroadcastingErrorEntity.Unknown;
                default:
                    throw new ArgumentException("Unknown switch", nameof(source));
            }
        }
        public static TransactionBroadcastingError ToDomain(this TransactionBroadcastingErrorEntity source)
        {
            switch (source)
            {
                case TransactionBroadcastingErrorEntity.FeeTooLow:
                    return TransactionBroadcastingError.FeeTooLow;
                case TransactionBroadcastingErrorEntity.NotEnoughBalance:
                    return TransactionBroadcastingError.NotEnoughBalance;
                case TransactionBroadcastingErrorEntity.RebuildRequired:
                    return TransactionBroadcastingError.RebuildRequired;
                case TransactionBroadcastingErrorEntity.RetryLater:
                    return TransactionBroadcastingError.RetryLater;
                case TransactionBroadcastingErrorEntity.TransientFailure:
                    return TransactionBroadcastingError.TransientFailure;
                case TransactionBroadcastingErrorEntity.Unknown:
                    return TransactionBroadcastingError.Unknown;
                default:
                    throw new ArgumentException("Unknown switch", nameof(source));
            }
        }
    }
}
