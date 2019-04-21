using System;
using System.Linq;
using Common;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models.Props;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models.Props.Payloads;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions.Mappers
{
    internal static class TransferCoinsTransactionExecutedPayloadMapper
    {
        public static TransactionEntity MapToDbEntity(this TransferCoinsTransactionExecutedEvent source, string blockchainType)
        {
            return new TransactionEntity
            {
                BlockId = source.BlockId,
                BlockchainType = blockchainType,
                Payload = new TransferCoinsTransactionExecutedPayload
                {
                    Fees = source.Fees,
                    IsIrreversible = source.IsIrreversible,
                    SpentCoins = source.SpentCoins,
                    ReceivedCoins = source.ReceivedCoins.Select(p => p.ToDbEntity())
                }.ToJson(),
                TransactionId = source.TransactionId,
                Type = TransactionType.TransferCoinsTransactionExecuted
            };
        }

        public static TransferCoinsTransactionExecutedEvent MapToCoinExecuted(this TransactionEntity source)
        {
            if (source.Type != TransactionType.TransferCoinsTransactionExecuted)
            {
                throw new ArgumentException($"Unable to cast {source.TransactionId} of {source.Type} to {nameof(TransferCoinsTransactionExecutedEvent)}");
            }

            var payload = source.Payload.DeserializeJson<TransferCoinsTransactionExecutedPayload>();

            return new TransferCoinsTransactionExecutedEvent(blockId: source.BlockId,
                transactionId: source.TransactionId,
                transactionNumber: source.TransactionNumber,
                receivedCoins: payload.ReceivedCoins.Select(p => p.ToDomain()).ToList(),
                spentCoins: payload.SpentCoins.ToList(),
                fees: payload.Fees?.ToList(),
                isIrreversible: payload.IsIrreversible);
        }
    }
}
