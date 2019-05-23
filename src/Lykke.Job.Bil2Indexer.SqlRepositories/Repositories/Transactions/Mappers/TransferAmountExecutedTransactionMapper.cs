using System;
using System.Linq;
using Common;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models.Props;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models.Props.Payloads;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions.Mappers
{
    internal static class TransferAmountExecutedTransactionMapper
    {
        public static TransactionEntity MapToDbEntity(this TransferAmountExecutedTransaction source, BlockId blockId)
        {
            return new TransactionEntity
            {
                BlockId = blockId,
                Payload = new TransferAmountExecutedTransactionPayload
                {
                    Fees = source.Fees,
                    IsIrreversible = source.IsIrreversible,
                    BalanceChanges = source.BalanceChanges.Select(p => p.ToDbEntity())
                }.ToJson(),
                TransactionId = source.TransactionId,
                TransactionNumber = source.TransactionNumber,
                Type = (int) TransactionType.TransferAmount
            };
        }

        public static TransferAmountExecutedTransaction MapToTransferAmountExecuted(this TransactionEntity source)
        {
            if ((TransactionType) source.Type != TransactionType.TransferAmount)
            {
                throw new ArgumentException($"Unable to map {source.TransactionId} of {source.Type} to {nameof(TransferAmountExecutedTransaction)}");
            }

            var payload = source.Payload.DeserializeJson<TransferAmountExecutedTransactionPayload>();

            return new TransferAmountExecutedTransaction
            (
                transactionId: source.TransactionId,
                transactionNumber: source.TransactionNumber,
                balanceChanges: payload.BalanceChanges.Select(p => p.ToDomain()).ToList(),
                fees: payload.Fees?.ToList(),
                isIrreversible: payload.IsIrreversible
            );
        }
    }
}
