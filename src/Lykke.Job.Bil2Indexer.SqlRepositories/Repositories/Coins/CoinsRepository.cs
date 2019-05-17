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
    public class CoinsRepository : ICoinsRepository
    {
        private readonly IPgConnectionStringProvider _connectionStringProvider;
        private readonly PostgreSQLCopyHelper<CoinEntity> _copyMapper;

        private readonly ILog _log;

        public CoinsRepository(IPgConnectionStringProvider connectionStringProvider,
            ILogFactory logFactory)
        {
            _connectionStringProvider = connectionStringProvider;
            _copyMapper = CoinCopyMapper.BuildCopyMapper();
            _log = logFactory.CreateLog(this);
        }

        public async Task AddIfNotExistsAsync(IReadOnlyCollection<Coin> coins)
        {
            var dbEntities = coins.Select(p => p.ToDbEntity()).ToList();

            if (!dbEntities.Any())
            {
                return;
            }

            var blockchainType = coins.First().BlockchainType;

            try
            {
                Copy(dbEntities, blockchainType);
            }
            catch (PostgresException e) when (e.IsNaturalKeyViolationException())
            {
                var notExisted = await ExcludeExistedInDbAsync(blockchainType, dbEntities);

                Copy(notExisted, blockchainType);
            }
        }

        private void Copy(IReadOnlyCollection<CoinEntity> dbEntities, string blockchainType)
        {
            if (!dbEntities.Any())
            {
                return;
            }

            using (var conn = new NpgsqlConnection(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                conn.Open();

                _copyMapper.SaveAll(conn, dbEntities);
            }
        }


        private async Task<IReadOnlyCollection<CoinEntity>> ExcludeExistedInDbAsync(string blockchainType, IReadOnlyCollection<CoinEntity> dbEntities)
        {
            var ids = dbEntities.Select(p => new CoinId(p.TransactionId, p.CoinNumber)).ToList();
            
            using (var db = new BlockchainDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var existedNaturalIds = (await db.Coins.Where(CoinPredicates.Build(ids))
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
            if (!ids.Any())
            {
                return;
            }

            using (var db = new BlockchainDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var foundCount = await db.Coins.Where(CoinPredicates.Build(ids))
                    .UpdateAsync(p => new CoinEntity { IsSpent = true });

                if (foundCount != ids.Count)
                {
                    throw new ArgumentException($"Not found entities to set spend. Passed: {ids.Count}, updated: {foundCount}");
                }
            }
        }

        public async Task RevertSpendingAsync(string blockchainType, IReadOnlyCollection<CoinId> ids)
        {
            if (!ids.Any())
            {
                return;
            }

            using (var db = new BlockchainDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var foundCount = await db.Coins
                    .Where(CoinPredicates.Build(ids))
                    .UpdateAsync(p => new CoinEntity {IsSpent = false});

                if (foundCount != ids.Count)
                {
                    _log.Info("Not all coins are reverted", context: new {foundCount, ids});
                }
            }
        }

        public async Task<IReadOnlyCollection<Coin>> GetSomeOfAsync(string blockchainType, IReadOnlyCollection<CoinId> ids)
        {
            if (!ids.Any())
            {
                return Array.Empty<Coin>();
            }

            using (var db = new BlockchainDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                return (await db.Coins.Where(CoinPredicates.Build(ids))
                        .ToListAsync())
                    .Select(p => p.ToDomain(blockchainType))
                    .ToList();
            }
        }

        public async Task RemoveIfExistAsync(string blockchainType, ISet<TransactionId> receivedInTransactionIds)
        {
            if (!receivedInTransactionIds.Any())
            {
                return;
            }

            using (var db = new BlockchainDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                await db.Coins
                        .Where(CoinPredicates.Build(receivedInTransactionIds))
                    .DeleteAsync();
            }
        }
    }
}
