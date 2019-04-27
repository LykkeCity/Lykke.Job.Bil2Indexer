using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.BalanceActions
{
    internal static class BalanceActionsPredicates
    {
        public static Expression<Func<BalanceActionEntity, bool>> Build(string blockchainType, IEnumerable<TransactionId> transactionIds)
        {
            return Build(blockchainType, transactionIds.Select(p => p.ToString()));
        }

        public static Expression<Func<BalanceActionEntity, bool>> Build(string blockchainType, IEnumerable<string> transactionIds, bool? isAssetAddressNull = null)
        {
            if (isAssetAddressNull == null)
            {
                return dbEntity => dbEntity.BlockchainType == blockchainType 
                                    && transactionIds.Contains(dbEntity.TransactionId);
            }

            if (isAssetAddressNull.Value)
            {
                return dbEntity => dbEntity.AssetAddress == null 
                                   && dbEntity.BlockchainType == blockchainType 
                                   && transactionIds.Contains(dbEntity.TransactionId);
            }
            else
            {
                return dbEntity => dbEntity.AssetAddress != null
                                   && dbEntity.BlockchainType == blockchainType
                                   && transactionIds.Contains(dbEntity.TransactionId);
            }
        }

        public static Expression<Func<BalanceActionEntity, bool>> Build(string blockchainType, BlockId blockId)
        {
            var stringBlockId = blockId.ToString();

            return dbEntity =>
                dbEntity.BlockchainType == blockchainType && dbEntity.BlockId == stringBlockId;
        }
    }
}
