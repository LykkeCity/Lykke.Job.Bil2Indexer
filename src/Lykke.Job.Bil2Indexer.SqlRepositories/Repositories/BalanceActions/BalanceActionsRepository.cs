using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly string _posgresConnstring;
        private readonly PostgreSQLCopyHelper<BalanceActionEntity> _copyMapper;

        private readonly IAssetInfosProvider _assetInfosProvider;

        public BalanceActionsRepository(string posgresConnstring, 
            IAssetInfosProvider assetInfosProvider)
        {
            _posgresConnstring = posgresConnstring;
            _assetInfosProvider = assetInfosProvider;
            
            _copyMapper = BalanceActionCopyMapper.BuildCopyMapper();
        }

        public async Task AddIfNotExistsAsync(string blockchainType,
            IReadOnlyCollection<BalanceAction> actions)
        {
            var dbEntities = actions.Select(domain => domain.ToDbEntity(blockchainType)).ToList();

            using (var conn = new NpgsqlConnection(_posgresConnstring))
            {
                conn.Open();

                try
                {
                    _copyMapper.SaveAll(conn, dbEntities);
                }
                catch (PostgresException e) when(e.IsNaturalKeyViolationException())
                {
                    var notExisted = await ExcludeExistedInDbAsync(dbEntities);

                    if (notExisted.Any())
                    {
                        _copyMapper.SaveAll(conn, notExisted);
                    }
                }
            }
        }

        private async Task<IReadOnlyCollection<BalanceActionEntity>> ExcludeExistedInDbAsync(IReadOnlyCollection<BalanceActionEntity> dbEntities)
        {
            if (dbEntities.GroupBy(p => p.BlockchainType).Count() > 1)
            {
                throw new ArgumentException("Unable to save batch with multiple blockchain type");
            }

            string BuildId(string bType, string transactionId, string address,  string assetId, string assetAddress)
            {
                return $"{bType}_{transactionId}_{address}_{assetId}_{assetAddress}";
            }

            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var blockchainType = dbEntities.First().BlockchainType;

                //force to use partial natural index
                
                var txIdsWithAssetAddress = dbEntities
                    .Where(p => p.AssetAddress != null)
                    .Select(p => p.TransactionId)
                    .ToList();

                var txIdsWithoutAssetAddress = dbEntities
                    .Where(p => p.AssetAddress == null)
                    .Select(p => p.TransactionId)
                    .ToList();

                var getNaturalIds1 = db.BalanceActions
                    .Where(BalanceActionsPredicates.Build(blockchainType, txIdsWithAssetAddress, isAssetAddressNull: false))
                    .Select(p => new { p.BlockchainType, p.TransactionId, p.Address, p.AssetId, p.AssetAddress })
                    .ToListAsync();

                var getNaturalIds2 = db.BalanceActions
                    .Where(BalanceActionsPredicates.Build(blockchainType, txIdsWithoutAssetAddress, isAssetAddressNull: true))
                    .Select(p => new { p.BlockchainType, p.TransactionId, p.Address, p.AssetId, p.AssetAddress })
                    .ToListAsync();

                await Task.WhenAll(getNaturalIds1, getNaturalIds2);

                var existedNaturalIds = getNaturalIds1.Result.Union(getNaturalIds2.Result)
                    .ToDictionary(p => BuildId(p.BlockchainType, p.TransactionId, p.Address, p.AssetId, p.AssetAddress));

                var dbEntitiesDic = dbEntities.ToDictionary(p =>
                    BuildId(p.BlockchainType, p.TransactionId, p.Address, p.AssetId, p.AssetAddress));

                return dbEntitiesDic.Where(p => !existedNaturalIds.ContainsKey(p.Key)).Select(p => p.Value).ToList();
            }
        }

        public async Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId)
        {
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                await db.BalanceActions
                    .Where(BalanceActionsPredicates.Build(blockchainType, blockId))
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
                    where  blockchain_type = @blockchainType 
                            and address = @address 
                            and block_number <= @blockNumber 
                            and asset_id = @assetId 
                            and asset_address {(isNullAssetAddress ? "is null" : "= @assetAddress")}";

            using (var conn = new NpgsqlConnection(_posgresConnstring))
            {
                var getSum = conn.QuerySingleAsync<string>(query, new { blockchainType ,
                    address = address.ToString(),
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
                    where  blockchain_type = @blockchainType 
                            and address = @address 
                            and block_number <= @blockNumber 
                    group by asset_id, asset_address";

            using (var conn = new NpgsqlConnection(_posgresConnstring))
            {
                var assetBalances = (await conn.QueryAsync<(string sum, string assetId, string assetAddress)>(query, new
                {
                    blockchainType,
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
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var queryRes = await db.BalanceActions.Where(BalanceActionsPredicates.Build(blockchainType, transactionIds))
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
    }
}
