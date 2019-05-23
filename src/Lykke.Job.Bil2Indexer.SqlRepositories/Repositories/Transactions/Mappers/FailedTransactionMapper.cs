using System;
using System.Linq;
using Common;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models.Props;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models.Props.Payloads;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions.Mappers
{
    internal static class FailedTransactionMapper
    {
        public static TransactionEntity MapToDbEntity(this FailedTransaction source, string blockchainType, string blockId)
        {
            return new TransactionEntity
            {
                BlockId = blockId,
                Payload = new FailedTransactionPayload
                {
                    Fees = source.Fees,
                    ErrorCode = source.ErrorCode.ToDbEntity(),
                    ErrorMessage = source.ErrorMessage

                }.ToJson(),
                TransactionId = source.TransactionId,
                TransactionNumber = source.TransactionNumber,
                Type = (int) TransactionType.Failed
            };
        }

        public static FailedTransaction MapToFailed(this TransactionEntity source)
        {
            if ((TransactionType) source.Type != TransactionType.Failed)
            {
                throw new ArgumentException($"Unable to map {source.TransactionId} of {source.Type} to {nameof(FailedTransaction)}");
            }

            var payload = source.Payload.DeserializeJson<FailedTransactionPayload>();

            return new FailedTransaction
            (
                transactionId: source.TransactionId,
                transactionNumber: source.TransactionNumber,
                errorCode: payload.ErrorCode.ToDomain(),
                errorMessage: payload.ErrorMessage,
                fees: payload.Fees?.ToList()
            );
        }
    }
}
