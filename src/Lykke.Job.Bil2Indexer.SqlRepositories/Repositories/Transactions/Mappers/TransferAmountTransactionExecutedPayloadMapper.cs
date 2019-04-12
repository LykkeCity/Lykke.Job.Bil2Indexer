using System.Linq;
using Common;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models.Props.Payloads;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions.Mappers
{
    public static class TransferAmountTransactionExecutedPayloadMapper
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
                    BalanceChanges = source.BalanceChanges
                }.ToJson(),
                TransactionId = source.TransactionId,
                Type = TransactionType.TransferAmountTransactionExecuted
            };
        }

        public static TransferAmountTransactionExecutedEvent MapToTransferExecuted(this TransactionEntity source)
        {
            var payload = source.Payload.DeserializeJson<TransferAmountTransactionExecutedPayload>();

            return new TransferAmountTransactionExecutedEvent(blockId: source.BlockId,
                transactionId: source.TransactionId,
                transactionNumber: source.TransactionNumber,
                balanceChanges: payload.BalanceChanges?.ToList(),
                fees: payload.Fees?.ToList(),
                isIrreversible: payload.IsIrreversible);
        }
    }
}
