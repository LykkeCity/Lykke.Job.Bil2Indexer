using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions
{
    internal class TransactionPredicates
    {
        public static Expression<Func<TransactionEntity, bool>> Build(BlockId blockId)
        {
            var stringBlockId = blockId.ToString();

            return p => p.BlockId == stringBlockId;
        }

        public static Expression<Func<TransactionEntity, bool>> Build(TransactionId transactionId)
        {
            var stringTransactionId = transactionId.ToString();

            return p => p.TransactionId == stringTransactionId;
        }

        public static Expression<Func<TransactionEntity, bool>> Build(IEnumerable<TransactionId> transactionIds)
        {
            var stringTransactionIds = transactionIds.Select(p => p.ToString());

            return p => stringTransactionIds.Contains(p.TransactionId);
        }
    }
}
