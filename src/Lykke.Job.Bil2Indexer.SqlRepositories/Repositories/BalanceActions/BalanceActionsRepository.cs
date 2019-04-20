using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.SharedDomain;
using Lykke.Common.Log;
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
                    
                    _copyMapper.SaveAll(conn, notExisted);
                }
            }
        }

        private async Task<IReadOnlyCollection<BalanceActionEntity>> ExcludeExistedInDbAsync(IReadOnlyCollection<BalanceActionEntity> dbEntities)
        {
            if (dbEntities.GroupBy(p => p.AssetId).Count() > 1)
            {
                throw new ArgumentException("Unable to save batch with multiple assetIds");
            }

            if (dbEntities.GroupBy(p => p.BlockchainType).Count() > 1)
            {
                throw new ArgumentException("Unable to save batch with multiple blockchain type");
            }

            string BuildId(string bType, string transactionId, string assetId)
            {
                return $"{bType}_{transactionId}_{assetId}";
            }

            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var blockchainType = dbEntities.First().BlockchainType;
                var txIds = dbEntities.Select(p => p.TransactionId).ToList();
                var assetId = dbEntities.First().AssetId;

                var query = db.BalanceActions
                        .Where(p => p.BlockchainType == blockchainType)
                        .Where(p => txIds.Contains(p.TransactionId))
                        .Where(p => p.AssetId == assetId)
                    .Select(p => new { p.BlockchainType, p.TransactionId, p.AssetId });

                var existedNaturalIds = (await query
                        .ToListAsync())
                    .ToDictionary(p => BuildId(p.BlockchainType, p.TransactionId, p.AssetId));

                var dbEntitiesDic = dbEntities.ToDictionary(p =>
                    BuildId(p.BlockchainType, p.TransactionId, p.AssetId));

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
                var queryRes = (await db.BalanceActions.Where(p =>
                        p.BlockchainType == blockchainType 
                        && p.BlockNumber <= atBlockNumber 
                        && p.Address == address 
                        && p.AssetId == asset.Id)
                    .GroupBy(p=>p.AssetId)
                    .Select(p => new
                    {
                        Sum = p.Sum(x => x.Value).ToString(CultureInfo.InvariantCulture),
                        Scale = p.First().ValueScale
                    }).ToListAsync()).FirstOrDefault();

                return queryRes != null ? MoneyHelper.BuildMoney(queryRes.Sum, queryRes.Scale) : Money.Parse("0");
            }
        }

        public async Task<IReadOnlyDictionary<Asset, Money>> GetBalancesAsync(string blockchainType, Address address, long atBlockNumber)
        {
            //TODO rewrite via dapper and plain sql
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var queryRes = await db.BalanceActions.Where(p =>
                        p.BlockchainType == blockchainType
                        && p.Address == address
                        && p.BlockNumber <= atBlockNumber)
                    .GroupBy(p => p.AssetId)
                    .Select(p => new
                    {
                        Sum = p.Sum(x => x.Value).ToString(CultureInfo.InvariantCulture),
                        Scale = p.First().ValueScale,
                        p.First().AssetId,
                        p.First().AssetAddress
                    }).ToListAsync();

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
                    return (IReadOnlyDictionary<AccountId, Money>)p.ToDictionary(
                        x => new AccountId(x.Address, new Asset(new AssetId(x.AssetId), new AssetAddress(x.AssetAddress))),
                        x => MoneyHelper.BuildMoney(x.Value, x.ValueScale));
                });
            }
        }
    }
}
