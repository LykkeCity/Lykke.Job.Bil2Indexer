using System;
using System.Linq.Expressions;
using LinqKit;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.IndexerState.Models;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.BlockHeaders
{
    internal static class BlockHeadersPredicates
    {
        public static Expression<Func<BlockHeaderEntity, bool>> Build(BlockId blockId)
        {
            var stringBlockId = blockId.ToString();

            return p => p.Id == stringBlockId;

        }

        public static Expression<Func<BlockHeaderEntity, bool>> Build(DateTime dateTime)
        {
            return p => p.MinedAt <= dateTime;

        }

        public static Expression<Func<BlockHeaderEntity, bool>> Build(long blockNumber)
        {
            return p => p.Number == blockNumber;
        }

        public static Expression<Func<BlockHeaderEntity, bool>> Build(long? startingAfterNumber, long? endingBeforeNumber)
        {
            var predicate = PredicateBuilder.New<BlockHeaderEntity>(p => true);
            if (startingAfterNumber != null)
            {
                // ReSharper disable once StringCompareToIsCultureSpecific
                predicate = predicate.And(p => p.Number > startingAfterNumber);
            }
            if (endingBeforeNumber != null)
            {
                // ReSharper disable once StringCompareToIsCultureSpecific
                predicate = predicate.And(p => p.Number < endingBeforeNumber);
            }

            return predicate;
        }
    }
}
