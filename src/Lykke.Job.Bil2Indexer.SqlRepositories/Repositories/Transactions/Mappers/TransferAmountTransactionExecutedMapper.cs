using System;
using System.Linq;
using Common;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models.Props;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models.Props.Payloads;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions.Mappers
{
    internal static class TransferAmountTransactionExecutedMapper
    {
        public static TransactionEntity MapToDbEntity(this TransferAmountTransactionExecutedEvent source, string blockchainType)
        {
            return new TransactionEntity
            {
                BlockId = source.BlockId,
                BlockchainType = blockchainType,
                Payload = new TransferAmountTransactionExecutedPayload
                {
                    Fees = source.Fees,
                    IsIrreversible = source.IsIrreversible,
                    BalanceChanges = source.BalanceChanges.Select(p => p.ToDbEntity())
                }.ToJson(),
                TransactionId = source.TransactionId,
                TransactionNumber = source.TransactionNumber,
                Type = TransactionType.TransferAmountTransactionExecuted
            };
        }

        public static TransferAmountTransactionExecutedEvent MapToTransferAmountExecuted(this TransactionEntity source)
        {
            if (source.Type != TransactionType.TransferAmountTransactionExecuted)
            {
                throw new ArgumentException($"Unable to cast {source.TransactionId} of {source.Type} to {nameof(TransferAmountTransactionExecutedEvent)}");
            }

            var payload = source.Payload.DeserializeJson<TransferAmountTransactionExecutedPayload>();

            return new TransferAmountTransactionExecutedEvent(blockId: source.BlockId,
                transactionId: source.TransactionId,
                transactionNumber: source.TransactionNumber,
                balanceChanges: payload.BalanceChanges.Select(p => p.ToDomain()).ToList(),
                fees: payload.Fees?.ToList(),
                isIrreversible: payload.IsIrreversible);
        }
    }
}
