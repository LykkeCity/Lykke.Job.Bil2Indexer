﻿using System;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models.Props;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions.Mappers
{
    internal static class TransactionMapper
    {
        public static TransactionEntity MapToDbEntity(this Domain.Transaction transaction)
        {
            switch (transaction.Type)
            {
                case Domain.TransactionType.TransferAmount:
                    return transaction.AsTransferAmount().MapToDbEntity(transaction.BlockId);
                case Domain.TransactionType.TransferCoins:
                    return transaction.AsTransferCoins().MapToDbEntity(transaction.BlockId);
                case Domain.TransactionType.Failed:
                    return transaction.AsFailed().MapToDbEntity(transaction.BlockchainType, transaction.BlockId);
                default:
                    throw new ArgumentOutOfRangeException(nameof(transaction.Type), transaction.Type, string.Empty);
            }
        }

        public static Domain.Transaction MapToTransactionEnvelope(this TransactionEntity entity, string blockchainType)
        {
            switch ((TransactionType) entity.Type)
            {
                case TransactionType.TransferAmount:
                    return new Domain.Transaction(blockchainType, entity.BlockId, entity.MapToTransferAmountExecuted());
                case TransactionType.TransferCoins:
                    return new Domain.Transaction(blockchainType, entity.BlockId, entity.MapToTransferCoinsExecuted());
                case TransactionType.Failed:
                    return new Domain.Transaction(blockchainType, entity.BlockId, entity.MapToFailed());
                default:
                    throw new ArgumentOutOfRangeException(nameof(entity.Type), entity.Type, string.Empty);
            }
        }
    }
}
