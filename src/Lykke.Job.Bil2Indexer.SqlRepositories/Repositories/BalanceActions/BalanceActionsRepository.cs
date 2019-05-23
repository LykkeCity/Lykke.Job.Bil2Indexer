using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Dapper;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Contract;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.BalanceActions.Mappers;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers;
using Lykke.Numerics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Z.EntityFramework.Plus;
using PostgreSQLCopyHelper;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.BalanceActions
{
    public class BalanceActionsRepository: IBalanceActionsRepository
    {
        private readonly IPgConnectionStringProvider _connectionStringProvider;
        private readonly PostgreSQLCopyHelper<BalanceActionEntity> _copyMapper;

        private readonly IAssetInfosProvider _assetInfosProvider;

        public BalanceActionsRepository(
            IPgConnectionStringProvider connectionStringProvider,
            IAssetInfosProvider assetInfosProvider)
        {
            _assetInfosProvider = assetInfosProvider;
            _connectionStringProvider = connectionStringProvider;

            _copyMapper = BalanceActionCopyMapper.BuildCopyMapper();
        }

        public async Task AddIfNotExistsAsync(string blockchainType,
            IEnumerable<BalanceAction> actions)
        {
            var dbEntities = actions.Select(domain => domain.ToDbEntity(blockchainType)).ToList();
            
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


        private void Copy(IReadOnlyCollection<BalanceActionEntity> dbEntities, string blockchainType)
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

        private async Task<IReadOnlyCollection<BalanceActionEntity>> ExcludeExistedInDbAsync(string blockchainType, IReadOnlyCollection<BalanceActionEntity> dbEntities)
        {
            string BuildId(string transactionId, string address,  string assetId, string assetAddress)
            {
                return $"{transactionId}_{address}_{assetId}_{assetAddress}";
            }

            //force to use partial natural index

            var txIdsWithAssetAddress = dbEntities
                .Where(p => p.AssetAddress != null)
                .Select(p => p.TransactionId);

            var txIdsWithoutAssetAddress = dbEntities
                .Where(p => p.AssetAddress == null)
                .Select(p => p.TransactionId);
            
            using (var db = new BlockchainDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var getNaturalIds1 = db.BalanceActions
                    .Where(BalanceActionsPredicates.Build(txIdsWithAssetAddress, isAssetAddressNull: false))
                    .Select(p => new { p.TransactionId, p.Address, p.AssetId, p.AssetAddress })
                    .ToListAsync();

                var getNaturalIds2 = db.BalanceActions
                    .Where(BalanceActionsPredicates.Build(txIdsWithoutAssetAddress, isAssetAddressNull: true))
                    .Select(p => new {p.TransactionId, p.Address, p.AssetId, p.AssetAddress })
                    .ToListAsync();

                await Task.WhenAll(getNaturalIds1, getNaturalIds2);

                var existedNaturalIds = getNaturalIds1.Result.Union(getNaturalIds2.Result)
                    .ToDictionary(p => BuildId(p.TransactionId, p.Address, p.AssetId, p.AssetAddress));

                var dbEntitiesDic = dbEntities.ToDictionary(p =>
                    BuildId(p.TransactionId, p.Address, p.AssetId, p.AssetAddress));

                return dbEntitiesDic.Where(p => !existedNaturalIds.ContainsKey(p.Key)).Select(p => p.Value).ToList();
            }
        }

        public async Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId)
        {
            using (var db = new BlockchainDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                await db.BalanceActions
                    .Where(BalanceActionsPredicates.Build(blockId))
                    .DeleteAsync();
            }
        }

        public async Task<Money> GetBalanceAsync(string blockchainType, Address address, Asset asset, long atBlockNumber)
        {
            var getAssetInfo = _assetInfosProvider.GetAsync(blockchainType, asset);

            var isNullAssetAddress = asset.Address == null;

            var query =
                  $@"select 
                        sum(value) :: text as sum
                    from balance_actions
                    where  address = @address 
                            and block_number <= @blockNumber 
                            and asset_id = @assetId 
                            and asset_address {(isNullAssetAddress ? "is null" : "= @assetAddress")}";

            using (var conn = new NpgsqlConnection(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var getSum = conn.QuerySingleAsync<string>(query, 
                    new { address = address.ToString(),
                        blockNumber = atBlockNumber,
                        assetId = asset.Id.ToString(),
                        assetAddress = asset.Address?.ToString()
                });

                await Task.WhenAll(getAssetInfo, getSum);

                return getSum.Result == null ? Money.Parse("0") :  MoneyHelper.BuildMoney(getSum.Result, getAssetInfo.Result.Scale);
            }
        }

        public async Task<IReadOnlyDictionary<Asset, Money>> GetBalancesAsync(string blockchainType, Address address, long atBlockNumber)
        {
            const string query =
                  @"select 
                        sum(value) :: text as sum, 
                        asset_id as assetId, 
                        asset_address as assetAddress
                    from balance_actions
                    where  address = @address 
                            and block_number <= @blockNumber 
                    group by asset_id, asset_address";

            using (var conn = new NpgsqlConnection(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var assetBalances = (await conn.QueryAsync<(string sum, string assetId, string assetAddress)>(query, new
                {
                    address = address.ToString(),
                    blockNumber = atBlockNumber
                })).ToDictionary(p => new Asset(new AssetId(p.assetId), p.assetAddress != null ? new AssetAddress(p.assetAddress): null), 
                    p => p.sum);

                var assetScales = (await _assetInfosProvider.GetSomeOfAsync(blockchainType, assetBalances.Select(p => p.Key)))
                    .ToDictionary(p => p.Asset, p => p.Scale);
                
                return assetBalances.ToDictionary(p => p.Key, p => MoneyHelper.BuildMoney(p.Value, assetScales[p.Key]));
            }
        }

        public async Task<IReadOnlyDictionary<TransactionId, IReadOnlyDictionary<AccountId, Money>>> GetSomeOfBalancesAsync(
            string blockchainType, 
            ISet<TransactionId> transactionIds)
        {
            if (!transactionIds.Any())
            {
                return new Dictionary<TransactionId, IReadOnlyDictionary<AccountId, Money>>();
            }

            using (var db = new BlockchainDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var queryRes = await db.BalanceActions.Where(BalanceActionsPredicates.Build(transactionIds))
                    .Select(p => new
                    {
                        p.TransactionId,
                        p.ValueString,
                        p.ValueScale,
                        p.AssetId,
                        p.AssetAddress,
                        p.Address
                    }).ToListAsync();


                return queryRes.GroupBy(p => new TransactionId(p.TransactionId)).ToDictionary(p => p.Key, p =>
                {
                    return (IReadOnlyDictionary<AccountId, Money>) p.ToDictionary
                    (
                        x => new AccountId
                        (
                            x.Address,
                            new Asset
                            (
                                new AssetId(x.AssetId),
                                x.AssetAddress != null
                                    ? new AssetAddress(x.AssetAddress)
                                    : null
                            )
                        ),
                        x => MoneyHelper.BuildMoney(x.ValueString, x.ValueScale)
                    );
                });
            }
        }

        public async Task<IReadOnlyCollection<BalanceAction>> GetCollectionAsync(string blockchainType, params TransactionId[] transactionIds)
        {
            using (var db = new BlockchainDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var entities = await db.BalanceActions.Where(BalanceActionsPredicates.Build(transactionIds))
                    .ToListAsync();

                return entities.Select(p => p.ToDomain(blockchainType)).ToList();
            }
        }

        public Task<IReadOnlyCollection<TransactionId>> GetTransactionsOfAddressAsync(string blockchainType, 
            Address address,
            int limit,
            bool orderAsc,
            TransactionId startingAfter,
            TransactionId endingBefore)
        {
            return GetTransactionIdsByPredicateAsync(blockchainType,
                BalanceActionsPredicates.Build(address),
                limit,
                orderAsc,
                startingAfter,
                endingBefore);
        }

        public Task<IReadOnlyCollection<TransactionId>> GetTransactionsOfBlockAsync(string blockchainType, 
            BlockId blockId, 
            int limit,
            bool orderAsc,
            TransactionId startingAfter,
            TransactionId endingBefore)
        {
            return GetTransactionIdsByPredicateAsync(blockchainType, 
                BalanceActionsPredicates.Build(blockId), 
                limit, 
                orderAsc,
                startingAfter, 
                endingBefore);
        }

        private async Task<IReadOnlyCollection<TransactionId>> GetTransactionIdsByPredicateAsync(string blockchainType,
            Expression<Func<BalanceActionEntity, bool>> predicate,
            int limit,
            bool orderAsc,
            TransactionId startingAfter,
            TransactionId endingBefore)
        {
            using (var db = new BlockchainDataContext(_connectionStringProvider.GetConnectionString(blockchainType)))
            {
                var query = db.BalanceActions
                    .Where(BalanceActionsPredicates.BuildEnumerationPredicate(predicate, startingAfter, endingBefore))
                    .Distinct()
                    .Take(limit);

                query = orderAsc ? query.OrderBy(p => p.TransactionId) : query.OrderByDescending(p => p.TransactionId);

                return (await query.Select(p => p.TransactionId)
                        .ToListAsync())
                    .Select(p=> new TransactionId(p)).ToList();
            }
        }
    }
}
