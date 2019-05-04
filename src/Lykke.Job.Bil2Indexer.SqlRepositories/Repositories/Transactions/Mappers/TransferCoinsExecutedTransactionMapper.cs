using System;
using System.Linq;
using Common;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models.Props;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models.Props.Payloads;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions.Mappers
{
    internal static class TransferCoinsExecutedTransactionMapper
    {
        public static TransactionEntity MapToDbEntity(this TransferCoinsExecutedTransaction source, string blockchainType, BlockId blockId)
        {
            return new TransactionEntity
            {
                BlockId = blockId,
                BlockchainType = blockchainType,
                Payload = new TransferCoinsExecutedTransactionPayload
                {
                    Fees = source.Fees,
                    IsIrreversible = source.IsIrreversible,
                    SpentCoins = source.SpentCoins,
                    ReceivedCoins = source.ReceivedCoins.Select(p => p.ToDbEntity())
                }.ToJson(),
                TransactionId = source.TransactionId,
                TransactionNumber = source.TransactionNumber,
                Type = (int) TransactionType.TransferCoins
            };
        }

        public static TransferCoinsExecutedTransaction MapToTransferCoinsExecuted(this TransactionEntity source)
        {
            if ((TransactionType) source.Type != TransactionType.TransferCoins)
            {
                throw new ArgumentException($"Unable to map {source.TransactionId} of {source.Type} to {nameof(TransferCoinsExecutedTransaction)}");
            }

            var payload = source.Payload.DeserializeJson<TransferCoinsExecutedTransactionPayload>();

            return new TransferCoinsExecutedTransaction
            (
                transactionId: source.TransactionId,
                transactionNumber: source.TransactionNumber,
                receivedCoins: payload.ReceivedCoins.Select(p => p.ToDomain()).ToList(),
                spentCoins: payload.SpentCoins.ToList(),
                fees: payload.Fees?.ToList(),
                isIrreversible: payload.IsIrreversible
            );
        }
    }
}
