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

        public static Expression<Func<BalanceActionEntity, bool>> Build(Address address, long maxBlockNumber)
        {
            var stringValue = address.ToString();

            return dbEntity => dbEntity.Address == stringValue && dbEntity.BlockNumber <= maxBlockNumber;
        }

        public static Expression<Func<BalanceActionEntity, bool>> Build(TransactionId transactionId)
        {
            var stringValue = transactionId.ToString();

            return dbEntity => (dbEntity.AssetAddress == null || dbEntity.AssetAddress != null) 
                               && dbEntity.TransactionId == stringValue;
        }

        public static Expression<Func<BalanceActionEntity, bool>> BuildEnumeration(
            Expression<Func<BalanceActionEntity, bool>> sourcePredicate,
            TransactionId startingAfter,
            TransactionId endingBefore,
            bool orderAsc)
        {
            return orderAsc? BuildAscEnumeration(sourcePredicate, startingAfter, endingBefore) 
                : BuildDescEnumeration(sourcePredicate, startingAfter, endingBefore);
        }

        private static Expression<Func<BalanceActionEntity, bool>> BuildAscEnumeration(
            Expression<Func<BalanceActionEntity, bool>> sourcePredicate,
            TransactionId startingAfter,
            TransactionId endingBefore)
        {
            var predicate = PredicateBuilder.New(sourcePredicate);
            if (startingAfter != null)
            {
                var stringValue = startingAfter.ToString();
                // ReSharper disable once StringCompareToIsCultureSpecific
                predicate = predicate.And(p => p.TransactionId.CompareTo(stringValue) > 0);
            }
            if (endingBefore != null)
            {
                var stringValue = endingBefore.ToString();
                // ReSharper disable once StringCompareToIsCultureSpecific
                predicate = predicate.And(p => p.TransactionId.CompareTo(stringValue) < 0);
            }

            return predicate;
        }

        private static Expression<Func<BalanceActionEntity, bool>> BuildDescEnumeration(
            Expression<Func<BalanceActionEntity, bool>> sourcePredicate,
            TransactionId startingAfter,
            TransactionId endingBefore)
        {
            var predicate = PredicateBuilder.New(sourcePredicate);
            if (startingAfter != null)
            {
                var stringValue = startingAfter.ToString();
                // ReSharper disable once StringCompareToIsCultureSpecific
                predicate = predicate.And(p => p.TransactionId.CompareTo(stringValue) < 0);
            }
            if (endingBefore != null)
            {
                var stringValue = endingBefore.ToString();
                // ReSharper disable once StringCompareToIsCultureSpecific
                predicate = predicate.And(p => p.TransactionId.CompareTo(stringValue) > 0);
            }

            return predicate;
        }
    }
}
