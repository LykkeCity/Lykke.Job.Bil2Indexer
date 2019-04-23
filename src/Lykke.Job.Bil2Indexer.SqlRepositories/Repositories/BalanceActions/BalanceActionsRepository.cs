﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
                catch (PostgresException e) when(e.IsUniqueConstraintViolationException())
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
                var txIds = dbEntities.Select(p => p.TransactionId).ToList();

                var query = db.BalanceActions
                        .Where(p => p.BlockchainType == blockchainType)
                        .Where(p => txIds.Contains(p.TransactionId))
                    .Select(p => new { p.BlockchainType, p.TransactionId, p.Address,  p.AssetId, p.AssetAddress });

                var existedNaturalIds = (await query
                        .ToListAsync())
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
                    .Where(p => p.BlockchainType == blockchainType && p.BlockId == blockId)
                    .DeleteAsync();
            }
        }

        public async Task<Money> GetBalanceAsync(string blockchainType, Address address, Asset asset, long atBlockNumber)
        {
            //TODO rewrite via dapper and plain sql
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var addresString = address.ToString();
                var assetIdString = asset.Id.ToString();
                var assetAddressString = asset.Address?.ToString();

                var queryRes = (await db.BalanceActions.Where(p =>
                        p.BlockchainType == blockchainType 
                        && p.BlockNumber <= atBlockNumber 
                        && p.Address == addresString
                        && p.AssetId == assetIdString
                        && p.AssetAddress == assetAddressString)
                    .ToListAsync())
                    .GroupBy(p=>p.AssetId)
                    .Select(p => new
                    {
                        Sum = p.Sum(x => x.Value).ToString(CultureInfo.InvariantCulture),
                        Scale = p.First().ValueScale
                    }).FirstOrDefault();

                return queryRes != null ? MoneyHelper.BuildMoney(queryRes.Sum, queryRes.Scale) : Money.Parse("0");
            }
        }

        public async Task<IReadOnlyDictionary<Asset, Money>> GetBalancesAsync(string blockchainType, Address address, long atBlockNumber)
        {
            //TODO rewrite via dapper and plain sql
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var addresString = address.ToString();

                var queryRes = (await db.BalanceActions.Where(p =>
                        p.BlockchainType == blockchainType
                        && p.Address == addresString
                        && p.BlockNumber <= atBlockNumber)
                        .ToListAsync())
                    .GroupBy(p => p.AssetId)
                    .Select(p => new
                    {
                        Sum = p.Sum(x => x.Value).ToString(CultureInfo.InvariantCulture),
                        Scale = p.First().ValueScale,
                        p.First().AssetId,
                        p.First().AssetAddress
                    });

                return queryRes.ToDictionary(
                    p => new Asset(new AssetId(p.AssetId), new AssetAddress(p.AssetAddress)),
                    p => MoneyHelper.BuildMoney(p.Sum, p.Scale));
            }
        }

        public async Task<IReadOnlyDictionary<TransactionId, IReadOnlyDictionary<AccountId, Money>>> GetSomeOfBalancesAsync(
            string blockchainType, 
            ISet<TransactionId> transactionIds)
        {
            //TODO rewrite via dapper and plain sql
            var ids = transactionIds.Select(p => p.ToString()).ToList();
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var queryRes = await db.BalanceActions.Where(p =>
                        p.BlockchainType == blockchainType
                        && ids.Any(x => x == p.TransactionId))
                    .Select(p => new
                    {
                        p.TransactionId,
                        Value = p.Value.ToString(CultureInfo.InvariantCulture),
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
                        x => MoneyHelper.BuildMoney(x.Value, x.ValueScale)
                    );
                });
            }
        }
    }
}
