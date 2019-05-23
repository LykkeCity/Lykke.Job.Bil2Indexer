using System;
using System.Linq.Expressions;
using LinqKit;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;

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

        public static Expression<Func<BlockHeaderEntity, bool>> BuildEnumeration(long maxBlockNumber, 
            long? startingAfterNumber, 
            long? endingBeforeNumber, 
            bool orderAsc)
        {
            return orderAsc
                ? BuildEnumerationAsc(maxBlockNumber, startingAfterNumber, endingBeforeNumber)
                : BuildEnumerationDesc(maxBlockNumber, startingAfterNumber, endingBeforeNumber);
        }

        private static Expression<Func<BlockHeaderEntity, bool>> BuildEnumerationAsc(long maxBlockNumber,
            long? startingAfterNumber, 
            long? endingBeforeNumber)
        {

            var predicate = PredicateBuilder.New<BlockHeaderEntity>(p=>p.Number <= maxBlockNumber);

            if (endingBeforeNumber != null)
            {
                predicate = predicate.And(p => p.Number < endingBeforeNumber);
            }
            if (startingAfterNumber != null)
            {
                predicate = predicate.And(p => p.Number > startingAfterNumber);
            }

            return predicate;
        }

        private static Expression<Func<BlockHeaderEntity, bool>> BuildEnumerationDesc(long maxBlockNumber,
            long? startingAfterNumber,
            long? endingBeforeNumber)
        {
            var predicate = PredicateBuilder.New<BlockHeaderEntity>(p => p.Number <= maxBlockNumber);

            if (startingAfterNumber != null)
            {
                predicate = predicate.And(p => p.Number < startingAfterNumber);
            }

            if (endingBeforeNumber != null)
            {
                predicate = predicate.And(p => p.Number > endingBeforeNumber);
            }

            return predicate;
        }
    }
}
