using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class TransactionModelMapper
    {
        public static IReadOnlyCollection<TransactionModel> ToViewModel(this IReadOnlyCollection<TransactionId> transactionIds, 
            IReadOnlyCollection<FeeEnvelope> fees,
            IReadOnlyCollection<BalanceAction> balances,
            long lastBlockNumber)
        {
            var feesPerTx = fees.ToLookup(p => p.TransactionId);
            var balancesPerTx = balances.ToLookup(p => p.TransactionId);

            return transactionIds.Select(p => p.ToViewModel(feesPerTx[p].ToList(),
                balancesPerTx[p].ToList(), lastBlockNumber)).ToList();
        }

        public static TransactionModel ToViewModel(this TransactionId transactionId, 
            IReadOnlyCollection<FeeEnvelope> fees,
            IReadOnlyCollection<BalanceAction> balances,  
            long lastBlockNumber)
        {
            var tx = balances.First();
            
            return new TransactionModel
            {
                Id = tx.TransactionId,
                BlockId = tx.BlockId,


                BlockNumber = tx.BlockNumber,
                Fees = fees.Select(p=> new FeeModel
                {
                    AssetId = new AssetIdModel
                    {
                        Address = p.Fee.Asset.Address,
                        Ticker = p.Fee.Asset.Id
                    },
                    Amount = p.Fee.Amount.ToString()
                }).ToArray(),

                ConfirmationsCount = lastBlockNumber - tx.BlockNumber,
                Transfers = balances.Select(p=> new TransferModel
                {
                    AssetId = new AssetIdModel
                    {
                        Address = p.AccountId.Asset.Address,
                        Ticker = p.AccountId.Asset.Id
                    },
                    Amount = p.Amount.ToString(),
                    Address = p.AccountId.Address,
                    TransferId = p.TransactionId
                }).ToArray(),

                //TODO
                Number = -1,
                //TODO
                IsIrreversible = true,
                //TODO
                Links = null,
            };
        }
    }
}
