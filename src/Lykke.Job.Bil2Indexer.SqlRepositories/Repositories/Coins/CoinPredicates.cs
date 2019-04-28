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
        public static Expression<Func<CoinEntity, bool>> Build(string blockchainType, IEnumerable<CoinId> ids, bool includeDeleted)
        {
            var coinIds = ids.Select(p => p.BuildCoinId()).ToList();

            if (includeDeleted)
            {
                return dbCoin => dbCoin.BlockchainType == blockchainType
                                 && coinIds.Contains(dbCoin.CoinId);
            }

            return dbCoin => dbCoin.BlockchainType == blockchainType
                             && coinIds.Contains(dbCoin.CoinId)
                             && !dbCoin.IsDeleted;
        }

        public static Expression<Func<CoinEntity, bool>> Build(string blockchainType, IEnumerable<TransactionId> txIds)
        {
            var stringTxIds = txIds.Select(p => p.ToString()).ToList();

            return dbCoin => dbCoin.BlockchainType == blockchainType
                             && stringTxIds.Contains(dbCoin.TransactionId);
        }
    }
}
