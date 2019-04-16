using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Bil2.SharedDomain;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Coins.Models;
using Lykke.Numerics;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Coins
{
    public class CoinsRepository:ICoinsRepository
    {
        private readonly string _posgresConnString;
        private readonly ILog _log;

        public CoinsRepository(string posgresConnString, ILogFactory logFactory)
        {
            _posgresConnString = posgresConnString;

            _log = logFactory.CreateLog(this);
        }

        public async Task AddIfNotExistsAsync(IReadOnlyCollection<Coin> coins)
        {
            using (var db = new BlockchainDataContext(_posgresConnString))
            {
                var dbEntities = coins.Select(Map).ToList();

                //TODO use COPY instead of insert
                await db.Coins.AddRangeAsync(dbEntities);

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException e)
                {
                    var ids = coins.Select(p => p.Id).ToList();
                    _log.Warning($"Entities already exists, {string.Join(", ", ids)}", exception: e);

                    var existedIds = (await db.Coins.Where(BuildPredicate(coins.First().BlockchainType, ids))
                            .Select(p => new { p.TransactionId, p.CoinNumber })
                            .ToListAsync())
                        .Select(p => new CoinId(p.TransactionId, p.CoinNumber))
                        .ToDictionary(p => p);
                    
                    var dbEntitiesDic = dbEntities.ToDictionary(p => new CoinId(p.TransactionId, p.CoinNumber));

                    foreach (var dbEntity in dbEntitiesDic.Where(p => existedIds.ContainsKey(p.Key)))
                    {
                        db.Entry(dbEntity.Value).State = EntityState.Detached;
                    }

                    await db.SaveChangesAsync();
                }
            }
        }
        
        public async Task SpendAsync(string blockchainType, IReadOnlyCollection<CoinId> ids)
        {
            using (var db = new BlockchainDataContext(_posgresConnString))
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
            using (var db = new BlockchainDataContext(_posgresConnString))
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
            using (var db = new BlockchainDataContext(_posgresConnString))
            {
                return (await db.Coins.Where(BuildPredicate(blockchainType, ids))
                        .Where(p => !p.IsDeleted).ToListAsync())
                    .Select(Map)
                    .ToList();
            }
        }

        public async Task RemoveIfExistAsync(string blockchainType, ISet<TransactionId> receivedInTransactionIds)
        {
            var ids = receivedInTransactionIds.Select(p => p.ToString()).ToList();

            using (var db = new BlockchainDataContext(_posgresConnString))
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

        private static CoinEntity Map(Coin source)
        {
            //throw new ArgumentException("Value mapping");

            return new CoinEntity
            {
                BlockchainType = source.BlockchainType,
                TransactionId = source.Id.TransactionId,
                CoinNumber = source.Id.CoinNumber,
                Address = source.Address,
                AddressNonce = source.AddressNonce,
                AddressTag = source.AddressTag,
                AddressTagType = Map(source.AddressTagType),
                AssetAddress = source.Asset.Address,
                IsSpent = source.IsSpent,
                AssetId = source.Asset.Id,
                ValueScale = source.Value.Scale
            };


        }

        private static Coin Map(CoinEntity source)
        {
            return new Coin(blockchainType: source.BlockchainType, 
                id: new CoinId(source.TransactionId, source.CoinNumber),
                asset: new Asset(new AssetId(source.AssetId), new AssetAddress(source.AssetAddress)), 
                address: source.Address, 
                value: new UMoney(new BigInteger(source.Value), source.ValueScale), 
                addressNonce: source.AddressNonce,
                addressTag: source.AddressTag,
                addressTagType: Map(source.AddressTagType), 
                isSpent: source.IsSpent );
        }

        private static AddressTagType? Map(CoinEntity.AddressTagTypeValues? source)
        {
            if (source == null)
            {
                return null;
            }

            switch (source.Value)
            {
                case CoinEntity.AddressTagTypeValues.Number:
                    return AddressTagType.Number;
                case CoinEntity.AddressTagTypeValues.Text:
                    return AddressTagType.Text;
                default:
                    throw new ArgumentException("Unknown mapping", nameof(source));
            }
        }

        private static CoinEntity.AddressTagTypeValues? Map(AddressTagType? source)
        {
            if (source == null)
            {
                return null;
            }

            switch (source.Value)
            {
                case AddressTagType.Number:
                    return CoinEntity.AddressTagTypeValues.Number;
                case AddressTagType.Text:
                    return CoinEntity.AddressTagTypeValues.Text;
                default:
                    throw new ArgumentException($"Unknown mapping", nameof(source));
            }
        }
    }
}
