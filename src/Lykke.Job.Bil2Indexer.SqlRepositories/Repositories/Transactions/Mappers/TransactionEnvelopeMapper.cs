using System;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models.Props;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions.Mappers
{
    internal static class TransactionEnvelopeMapper
    {
        public static TransactionEnvelope MapToTransactionEnvelope(this TransactionEntity entity)
        {
            switch (entity.Type)
            {
                case TransactionType.TransferAmountTransactionExecuted:
                    return new TransactionEnvelope(entity.MapToTransferAmountExecuted());
                case TransactionType.TransferCoinsTransactionExecuted:
                    return new TransactionEnvelope(entity.MapToCoinExecuted());
                case TransactionType.TransactionFailed:
                    return new TransactionEnvelope(entity.MapToFailed());
                default:
                    throw new ArgumentOutOfRangeException(nameof(entity.Type), entity.Type, string.Empty);
            }
        }
    }
}