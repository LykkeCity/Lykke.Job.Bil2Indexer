﻿using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.BalanceActions.Mappers
{
    internal static class BalanceActionDbEntityMapper
    {
        public static BalanceActionEntity ToDbEntity(this BalanceAction source, string blockchainType)
        {
            return new BalanceActionEntity
            {
                TransactionId = source.TransactionId,
                AssetAddress = source.AccountId.Asset.Address,
                AssetId = source.AccountId.Asset.Id,
                BlockId = source.BlockId,
                BlockNumber = source.BlockNumber,
                ValueScale = source.Amount.Scale,
                ValueString = MoneyHelper.BuildPgString(source.Amount),
                Address = source.AccountId.Address,
                ValueMoney = source.Amount
            };
        }
    }
}
