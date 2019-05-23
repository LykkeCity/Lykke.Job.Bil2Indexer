using System;
using System.Linq.Expressions;
using LinqKit;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.AssetInfos
{
    internal static class AssetInfosPredicates
    {
        public static Expression<Func<AssetInfoEntity, bool>> Build(Asset asset)
        {
            var id = asset.BuildId();

            return dbEntity => dbEntity.Id == id;
        }

        public static Expression<Func<AssetInfoEntity, bool>> BuildEnumeration(Asset startingAfter, Asset endingBefore, bool orderAsc)
        {
            return orderAsc
                ? BuildEnumerationAsc(startingAfter, endingBefore)
                : BuildEnumerationDesc(startingAfter, endingBefore);
        }

        private static Expression<Func<AssetInfoEntity, bool>> BuildEnumerationAsc(Asset startingAfter, Asset endingBefore)
        {
            var predicate = PredicateBuilder.New<AssetInfoEntity>(p => true);
            if (startingAfter != null)
            {
                var stringValue = startingAfter.BuildId();
                // ReSharper disable once StringCompareToIsCultureSpecific
                predicate = predicate.And(p => p.Id.CompareTo(stringValue) > 0);
            }
            if (endingBefore != null)
            {
                var stringValue = endingBefore.BuildId();
                // ReSharper disable once StringCompareToIsCultureSpecific
                predicate = predicate.And(p => p.Id.CompareTo(stringValue) < 0);
            }

            return predicate;
        }

        private static Expression<Func<AssetInfoEntity, bool>> BuildEnumerationDesc(Asset startingAfter, Asset endingBefore)
        {
            var predicate = PredicateBuilder.New<AssetInfoEntity>(p => true);
            if (startingAfter != null)
            {
                var stringValue = startingAfter.BuildId();
                // ReSharper disable once StringCompareToIsCultureSpecific
                predicate = predicate.And(p => p.Id.CompareTo(stringValue) > 0);
            }
            if (endingBefore != null)
            {
                var stringValue = endingBefore.BuildId();
                // ReSharper disable once StringCompareToIsCultureSpecific
                predicate = predicate.And(p => p.Id.CompareTo(stringValue) < 0);
            }

            return predicate;
        }
    }
}
