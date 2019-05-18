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
        public static IReadOnlyCollection<TransactionModel> ToViewModel(this IReadOnlyCollection<Transaction> source, 
            IReadOnlyCollection<FeeEnvelope> fees,
            IReadOnlyCollection<BalanceAction> balances,
            int lastBlockNumber)
        {
            var feesPerTx = fees.ToLookup(p => p.TransactionId);
            var balancesPerTx = balances.ToLookup(p => p.TransactionId);

            return source.Select(p =>
            {
                var txId = p.TransactionId();

                return p.ToViewModel(feesPerTx[txId].ToList(),
                    balancesPerTx[txId].ToList(), lastBlockNumber);
            }).ToList();
        }

        public static TransactionModel ToViewModel(this Transaction source, IReadOnlyCollection<FeeEnvelope> fees,
            IReadOnlyCollection<BalanceAction> balances,  int lastBlockNumber)
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
                Number = source.TransactionNumber(),
                //TODO
                IsIrreversible = true,
                //TODO
                Links = null,
            };
        }


        private static int TransactionNumber(this Transaction source)
        {
            if (source.IsTransferAmount)
            {
                return source.AsTransferAmount().TransactionNumber;
            }

            if (source.IsTransferCoins)
            {
                return source.AsTransferCoins().TransactionNumber;
            }

            if (source.IsFailed)
            {
                return source.AsFailed().TransactionNumber;
            }

            throw new ArgumentException($"Unknown tx type {source.Type}");
        }

        private static TransactionId TransactionId(this Transaction source)
        {
            if (source.IsTransferAmount)
            {
                return source.AsTransferAmount().TransactionId;
            }

            if (source.IsTransferCoins)
            {
                return source.AsTransferCoins().TransactionId;
            }

            if (source.IsFailed)
            {
                return source.AsFailed().TransactionId;
            }

            throw new ArgumentException($"Unknown tx type {source.Type}");
        }
    }
}
