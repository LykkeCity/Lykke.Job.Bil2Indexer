using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly PostgreSQLCopyHelper<CoinEntity> _copyMapper;

        private readonly ILog _log;

        public CoinsRepository(string posgresConnString, ILogFactory logFactory)
        {
            _posgresConnstring = posgresConnString;
            
            _copyMapper = CoinCopyMapper.BuildCopyMapper();
            _log = logFactory.CreateLog(this);
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
                catch (PostgresException e) when (e.IsNaturalKeyViolationException())
                {
                    var notExisted = await ExcludeExistedInDbAsync(dbEntities);

                    if (notExisted.Any())
                    {
                        _copyMapper.SaveAll(conn, notExisted);
                    }
                }
            }
        }


        private async Task<IReadOnlyCollection<CoinEntity>> ExcludeExistedInDbAsync(IReadOnlyCollection<CoinEntity> dbEntities)
        {
            if (dbEntities.GroupBy(p => p.BlockchainType).Count() > 1)
            {
                throw new ArgumentException("Unable to save batch with multiple blockchain type");
            }

            var ids = dbEntities.Select(p => new CoinId(p.TransactionId, p.CoinNumber)).ToList();
            
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var existedNaturalIds = (await db.Coins.Where(CoinPredicates.Build(dbEntities.First().BlockchainType, ids, includeDeleted: true))
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
                var foundCount = await db.Coins.Where(CoinPredicates.Build(blockchainType, ids, includeDeleted: false))
                    .UpdateAsync(p => new CoinEntity { IsSpent = true });

                if (foundCount != ids.Count)
                {
                    throw new ArgumentException($"Not found entities to set spend. Passed: {ids.Count}, updated: {foundCount}");
                }
            }
        }

        public async Task RevertSpendingAsync(string blockchainType, IReadOnlyCollection<CoinId> ids)
        {
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var foundCount = await db.Coins
                    .Where(CoinPredicates.Build(blockchainType, ids, includeDeleted: true))
                    .UpdateAsync(p => new CoinEntity {IsSpent = false});

                if (foundCount != ids.Count)
                {
                    _log.Info("Not all coins are reverted", context: new {foundCount, ids});
                }
            }
        }

        public async Task<IReadOnlyCollection<Coin>> GetSomeOfAsync(string blockchainType, IReadOnlyCollection<CoinId> ids)
        {
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                return (await db.Coins.Where(CoinPredicates.Build(blockchainType, ids, includeDeleted:false))
                        .ToListAsync())
                    .Select(p => p.ToDomain())
                    .ToList();
            }
        }

        public async Task RemoveIfExistAsync(string blockchainType, ISet<TransactionId> receivedInTransactionIds)
        {
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                await db.Coins
                        .Where(CoinPredicates.Build(blockchainType, receivedInTransactionIds))
                    .UpdateAsync(p => new CoinEntity {IsDeleted = true});
            }
        }
    }
}
