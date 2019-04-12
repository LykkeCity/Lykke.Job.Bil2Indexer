using System.Linq;
using Common;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models.Props.Payloads;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions.Mappers
{
    public static class TransferCoinsTransactionExecutedPayloadMapper
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
                    ReceivedCoins = source.ReceivedCoins
                }.ToJson(),
                TransactionId = source.TransactionId,
                Type = TransactionType.TransferCoinsTransactionExecuted
            };
        }

        public static TransferCoinsTransactionExecutedEvent MapToCoinExecuted(this TransactionEntity source)
        {
            var payload = source.Payload.DeserializeJson<TransferCoinsTransactionExecutedPayload>();

            return new TransferCoinsTransactionExecutedEvent(blockId: source.BlockId,
                transactionId: source.TransactionId,
                transactionNumber: source.TransactionNumber,
                receivedCoins: payload.ReceivedCoins?.ToList(),
                spentCoins: payload.SpentCoins?.ToList(),
                fees: payload.Fees?.ToList(),
                isIrreversible: payload.IsIrreversible);
        }
    }
}
