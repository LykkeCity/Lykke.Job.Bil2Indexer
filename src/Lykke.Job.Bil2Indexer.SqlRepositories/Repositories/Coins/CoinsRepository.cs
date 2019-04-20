using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Bil2.SharedDomain;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Coins.Mappers;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Z.EntityFramework.Plus;
using PostgreSQLCopyHelper;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Coins
{
    public class CoinsRepository:ICoinsRepository
    {
        private readonly string _posgresConnstring;
        private readonly ILog _log;
        private readonly PostgreSQLCopyHelper<CoinEntity> _copyMapper;

        public CoinsRepository(string posgresConnString, ILogFactory logFactory)
        {
            _posgresConnstring = posgresConnString;

            _log = logFactory.CreateLog(this);
            _copyMapper = CoinCopyMapper.BuildCopyMapper();
        }

        public async Task AddIfNotExistsAsync(IReadOnlyCollection<Coin> coins)
        {
            if (coins.GroupBy(p => p.BlockchainType).Count() > 1)
            {
                throw new ArgumentException($"Unable to save coins with different {nameof(Coin.BlockchainType)} in single batch", nameof(coins));
            }

            var dbEntities = coins.Select(p => p.ToDbEntity()).ToList();

            using (var conn = new NpgsqlConnection(_posgresConnstring))
            {
                conn.Open();

                try
                {
                    _copyMapper.SaveAll(conn, dbEntities);
                }
                catch (PostgresException e) when (e.IsConstraintViolationException())
                {
                    var notExisted = await ExcludeExistedInDbAsync(dbEntities);
                    _log.Warning($"Entities already exist, fallback adding {notExisted.Count} of {dbEntities.Count}", exception: e);

                    _copyMapper.SaveAll(conn, notExisted);
                }
            }
        }


        private async Task<IReadOnlyCollection<CoinEntity>> ExcludeExistedInDbAsync(IReadOnlyCollection<CoinEntity> dbEntities)
        {
            var ids = dbEntities.Select(p => new CoinId(p.TransactionId, p.CoinNumber)).ToList();
            
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var existedNaturalIds = (await db.Coins.Where(BuildPredicate(dbEntities.First().BlockchainType, ids))
                        .Select(p => new { p.TransactionId, p.CoinNumber })
                        .ToListAsync())
                    .Select(p => new CoinId(p.TransactionId, p.CoinNumber))
                    .ToDictionary(p => p);

                var dbEntitiesDic = dbEntities.ToDictionary(p => new CoinId(p.TransactionId, p.CoinNumber));

                return dbEntitiesDic.Where(p => !existedNaturalIds.ContainsKey(p.Key)).Select(p => p.Value).ToList();
            }
        }

        public async Task SpendAsync(string blockchainType, IReadOnlyCollection<CoinId> ids)
        {
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var foundCount = await db.Coins.Where(BuildPredicate(blockchainType, ids)).UpdateAsync(p => new CoinEntity { IsSpent = true });

                if (foundCount != ids.Count)
                {
                    var notFound = (await db.Coins.Where(BuildNotPredicate(blockchainType, ids))
                            .Select(p => new { p.TransactionId, p.CoinNumber }).ToListAsync())
                        .Select(p => new CoinId(p.TransactionId, p.CoinNumber))
                        .ToDictionary(p => p);

                    var notFoundIds = ids.Where(p => notFound.ContainsKey(p)).ToList();

                    throw new ArgumentException($"Not found entities to set spend. Passed: {ids.Count}, updated: {foundCount}, not found {notFoundIds.ToJson()}");
                }
            }
        }

        public async Task RevertSpendingAsync(string blockchainType, IReadOnlyCollection<CoinId> ids)
        {
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var foundCount = await db.Coins.Where(BuildPredicate(blockchainType, ids)).UpdateAsync(p => new CoinEntity {IsSpent = false});

                if (foundCount != ids.Count)
                {
                    var notFound = (await db.Coins.Where(BuildNotPredicate(blockchainType, ids))
                            .Select(p => new {p.TransactionId, p.CoinNumber}).ToListAsync())
                        .Select(p => new CoinId(p.TransactionId, p.CoinNumber))
                        .ToDictionary(p => p);

                    var notFoundIds = ids.Where(p => notFound.ContainsKey(p)).ToList();

                    throw new ArgumentException($"Not found entities to revert spend. Passed: {ids.Count}, updated: {foundCount}, not found {notFoundIds.ToJson()}");
                }
            }
        }

        public async Task<IReadOnlyCollection<Coin>> GetSomeOfAsync(string blockchainType, IReadOnlyCollection<CoinId> ids)
        {
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                return (await db.Coins.Where(BuildPredicate(blockchainType, ids))
                        .Where(p => !p.IsDeleted).ToListAsync())
                    .Select(p=> p.ToDomain())
                    .ToList();
            }
        }

        public async Task RemoveIfExistAsync(string blockchainType, ISet<TransactionId> receivedInTransactionIds)
        {
            var ids = receivedInTransactionIds.Select(p => p.ToString()).ToList();

            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                await db.Coins.Where(p => ids.Any(x => x == p.TransactionId))
                    .UpdateAsync(p => new CoinEntity {IsDeleted = true});
            }
        }

        private Expression<Func<CoinEntity, bool>> BuildPredicate(string blockchainType, IReadOnlyCollection<CoinId> ids)
        {
            return dbCoin => dbCoin.BlockchainType == blockchainType 
                             && ids.Any(coinRef => coinRef.TransactionId == dbCoin.TransactionId  
                                                   && coinRef.CoinNumber == dbCoin.CoinNumber);
        }

        private Expression<Func<CoinEntity, bool>> BuildNotPredicate(string blockchainType, IReadOnlyCollection<CoinId> ids)
        {
            return dbCoin => dbCoin.BlockchainType == blockchainType
                             && !ids.Any(coinRef => coinRef.TransactionId == dbCoin.TransactionId
                                                   && coinRef.CoinNumber == dbCoin.CoinNumber);
        }
    }
}
