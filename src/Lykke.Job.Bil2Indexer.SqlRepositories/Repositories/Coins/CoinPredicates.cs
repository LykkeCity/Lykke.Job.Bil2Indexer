using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Coins
{
    public static class CoinPredicates
    {
        public static Expression<Func<CoinEntity, bool>> Build(IEnumerable<CoinId> ids)
        {
            var coinIds = ids.Select(p => p.BuildCoinId());
            
            return dbCoin => coinIds.Contains(dbCoin.CoinId);
        }

        public static Expression<Func<CoinEntity, bool>> Build(IEnumerable<TransactionId> txIds)
        {
            var stringTxIds = txIds.Select(p => p.ToString());

            return dbCoin => stringTxIds.Contains(dbCoin.TransactionId);
        }

        public static Expression<Func<CoinEntity, bool>> Build(BlockId blockId)
        {
            return dbCoin => dbCoin.BlockId == blockId;
        }
    }
}
