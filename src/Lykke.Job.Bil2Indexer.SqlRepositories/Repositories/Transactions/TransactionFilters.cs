using System.Collections.Generic;
using System.Linq;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions
{
    internal static class TransactionFilters
    {
        public static IQueryable<TransactionEntity> FilterByIds(this IQueryable<TransactionEntity> query, string blockchainType, IEnumerable<string> ids)
        {
            return query.Where(t => t.BlockchainType == blockchainType && ids.Contains(t.TransactionId));
        }
    }
}
