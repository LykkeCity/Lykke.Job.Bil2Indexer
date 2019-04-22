using System;
using System.Linq;
using Common;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models.Props;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models.Props.Payloads;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions.Mappers
{
    internal static class TransactionFailedEventPayloadMapper
    {
        public static TransactionEntity MapToDbEntity(this TransactionFailedEvent source, string blockchainType)
        {
            return new TransactionEntity
            {
                BlockId = source.BlockId,
                BlockchainType =  blockchainType,
                Payload = new TransactionFailedEventPayload
                {
                    Fees = source.Fees,
                    ErrorCode = source.ErrorCode.ToDbEntity(),
                    ErrorMessage = source.ErrorMessage

                }.ToJson(),
                TransactionId = source.TransactionId,
                TransactionNumber = source.TransactionNumber,
                Type = TransactionType.TransactionFailed
            };
        }

        public static TransactionFailedEvent MapToFailed(this TransactionEntity source)
        {
            if (source.Type != TransactionType.TransactionFailed)
            {
                throw new ArgumentException($"Unable to cast {source.TransactionId} of {source.Type} to {nameof(TransactionFailedEvent)}");
            }

            var payload = source.Payload.DeserializeJson<TransactionFailedEventPayload>();

            return new TransactionFailedEvent(blockId: source.BlockId,
                transactionId: source.TransactionId,
                transactionNumber: source.TransactionNumber,
                errorCode: payload.ErrorCode.ToDomain(),
                errorMessage: payload.ErrorMessage,
                fees: payload.Fees?.ToList());
        }
    }
}
