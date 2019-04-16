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
using Lykke.Numerics;
using Microsoft.EntityFrameworkCore;
using Z.EntityFramework.Plus;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.BalanceActions
{
    public class BalanceActionsRepository: IBalanceActionsRepository
    {
        private readonly string _posgresConnstring;
        private readonly ILog _log;

        public BalanceActionsRepository(string posgresConnstring, ILogFactory logFactory)
        {
            _posgresConnstring = posgresConnstring;

            _log = logFactory.CreateLog(this);
        }

        public async Task AddIfNotExistAsync(string blockchainType, 
            IEnumerable<BalanceAction> actions)
        {
            using (var db = new BlockchainDataContext(_posgresConnstring))
            {
                var dbEntities = actions.Select(p => Map(p, blockchainType)).ToList();
                
                //TODO use COPY instead of insert
                await db.BalanceActions.AddRangeAsync(dbEntities);

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateException e)
                {
                    string BuildId(string bType, string transactionId, string assetId)
                    {
                        return $"{bType}_{transactionId}_{assetId}";
                    }

                    var ids = dbEntities.Select(p => BuildId(p.BlockchainType, p.TransactionId, p.AssetId))
                        .ToList();

                    _log.Warning($"Entities already exists, {string.Join(", ", ids)}", exception: e);

                    var savedIdData = dbEntities
                        .Select(p => new { p.BlockchainType, p.TransactionId, p.AssetId })
                        .ToList();

                    var existedIds = (await db.BalanceActions
                            .Where(dbEntity => savedIdData.Any(
                                sd => sd.BlockchainType == dbEntity.BlockchainType
                                      && sd.TransactionId == dbEntity.TransactionId
                                      && sd.AssetId == dbEntity.AssetId))
                            .Select(p => new { p.BlockchainType, p.TransactionId, p.AssetId })
                            .ToListAsync())
                        .ToDictionary(p => BuildId(p.BlockchainType, p.TransactionId, p.AssetId));


                    var dbEntitiesDic = dbEntities.ToDictionary(p =>
                        BuildId(p.BlockchainType, p.TransactionId, p.AssetId));

                    foreach (var dbEntity in dbEntitiesDic.Where(p => existedIds.ContainsKey(p.Key)))
                    {
                        db.Entry(dbEntity.Value).State = EntityState.Detached;
                    }

                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task TryRemoveAllOfBlockAsync(string blockchainType, string blockId)
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
                        Sum = p.Sum(x => x.Value).ToString(),
                        Scale = p.First().ValueScale
                    }).ToListAsync()).FirstOrDefault();

                return queryRes != null ? Money.Parse(queryRes.Sum) : Money.Parse("0");
            }
        }

        public Task<IReadOnlyDictionary<Asset, Money>> GetBalancesAsync(string blockchainType, Address address, long atBlockNumber)
        {
            throw new NotImplementedException();
        }

        private static BalanceActionEntity Map(BalanceAction source, string blockchainType)
        {
            return new BalanceActionEntity
            {
                BlockchainType = blockchainType,
                TransactionId = source.TransactionId,
                AssetAddress = source.Asset.Address,
                AssetId = source.Asset.Id,
                BlockId = source.BlockId,
                BlockNumber = source.BlockNumber,
                ValueScale = source.Amount.Scale,
                Value = 1,
                //Value = ??
                Address = source.Address,
            };
        }
    }
}
