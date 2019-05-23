using System.Collections.Generic;
using System.Linq;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Service.Bil2IndexerWebApi.Extensions;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class TransactionModelMapper
    {
        public static IReadOnlyCollection<TransactionResponce> ToViewModel(this IReadOnlyCollection<TransactionId> transactionIds, 
            IReadOnlyCollection<FeeEnvelope> fees,
            IReadOnlyCollection<BalanceAction> balances,
            long lastBlockNumber,
            IUrlHelper url,
            string blockchainType)
        {
            var feesPerTx = fees.ToLookup(p => p.TransactionId);
            var balancesPerTx = balances.ToLookup(p => p.TransactionId);

            return transactionIds
                .Select(p => p.ToViewModel(feesPerTx[p].ToList(),balancesPerTx[p].ToList(), lastBlockNumber, url, blockchainType))
                .ToList();
        }

        public static TransactionResponce ToViewModel(this TransactionId transactionId, 
            IReadOnlyCollection<FeeEnvelope> fees,
            IReadOnlyCollection<BalanceAction> balances,  
            long lastBlockNumber,
            IUrlHelper url,
            string blockchainType)
        {
            var tx = balances.First();

            if (tx.BlockNumber >= lastBlockNumber)
            {
                return null;
            }
            
            return new TransactionResponce
            {
                Id = tx.TransactionId,
                BlockId = tx.BlockId,


                BlockNumber = tx.BlockNumber,
                Fees = fees.Select(p=> new FeeModel
                {
                    AssetId = new AssetIdResponce
                    {
                        Address = p.Fee.Asset.Address,
                        Ticker = p.Fee.Asset.Id
                    },
                    Amount = p.Fee.Amount.ToString()
                }).ToArray(),

                ConfirmationsCount = lastBlockNumber - tx.BlockNumber,
                Transfers = balances.Select(p=> new TransferResponce
                {
                    AssetId = new AssetIdResponce
                    {
                        Address = p.AccountId.Asset.Address,
                        Ticker = p.AccountId.Asset.Id,
                        Id = p.AccountId.Asset.BuildId()
                    },
                    Amount = p.Amount.ToString(),
                    Address = p.AccountId.Address,
                    TransferId = p.TransactionId
                }).ToArray(),
                //TODO
                IsIrreversible = true,
                //TODO
                Links = new TransactionLinks
                {
                    BlockUrl = url.BlockUrl(blockchainType, tx.BlockId),
                    RawUrl = url.RawTransactionUrl(blockchainType, tx.TransactionId),
                },
            };
        }
    }
}
