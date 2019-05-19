using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqKit;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.BalanceActions
{
    internal static class BalanceActionsPredicates
    {
        public static Expression<Func<BalanceActionEntity, bool>> Build(IEnumerable<TransactionId> transactionIds)
        {
            return Build(transactionIds.Select(p => p.ToString()));
        }

        public static Expression<Func<BalanceActionEntity, bool>> Build(IEnumerable<string> transactionIds, bool? isAssetAddressNull = null)
        {
            if (isAssetAddressNull == null)
            {
                return dbEntity => (dbEntity.AssetAddress == null || dbEntity.AssetAddress != null) //force to use filtered index
                                   && transactionIds.Contains(dbEntity.TransactionId);
            }

            if (isAssetAddressNull.Value)
            {
                return dbEntity => dbEntity.AssetAddress == null 
                                   && transactionIds.Contains(dbEntity.TransactionId);
            }
            else
            {
                return dbEntity => dbEntity.AssetAddress != null
                                   && transactionIds.Contains(dbEntity.TransactionId);
            }
        }

        public static Expression<Func<BalanceActionEntity, bool>> Build(BlockId blockId)
        {
            var stringBlockId = blockId.ToString();

            return dbEntity => dbEntity.BlockId == stringBlockId;
        }

        public static Expression<Func<BalanceActionEntity, bool>> Build(Address address)
        {
            var stringValue = address.ToString();

            return dbEntity => dbEntity.Address == stringValue;
        }

        public static Expression<Func<BalanceActionEntity, bool>> Build(TransactionId transactionId)
        {
            var stringValue = transactionId.ToString();

            return dbEntity => (dbEntity.AssetAddress == null || dbEntity.AssetAddress != null) 
                               && dbEntity.TransactionId == stringValue;
        }

        public static Expression<Func<BalanceActionEntity, bool>> BuildEnumerationPredicate(
            Expression<Func<BalanceActionEntity, bool>> sourcePredicate,
            string startingAfter,
            string endingBefore)
        {
            var predicate = PredicateBuilder.New(sourcePredicate);
            if (!string.IsNullOrEmpty(startingAfter))
            {
                // ReSharper disable once StringCompareToIsCultureSpecific
                predicate = predicate.And(p => p.TransactionId.CompareTo(startingAfter) > 0);
            }
            if (!string.IsNullOrEmpty(endingBefore))
            {
                // ReSharper disable once StringCompareToIsCultureSpecific
                predicate = predicate.And(p => p.TransactionId.CompareTo(endingBefore) < 0);
            }

            return predicate;
        }
    }
}
