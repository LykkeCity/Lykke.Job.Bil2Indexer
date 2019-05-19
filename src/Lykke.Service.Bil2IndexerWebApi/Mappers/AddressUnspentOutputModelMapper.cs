using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class AddressUnspentOutputModelMapper
    {
        public static IReadOnlyCollection<AddressUnspentOutputModel> ToViewModel(this IReadOnlyCollection<Coin> source, long lastBlockNumber)
        {
            return source.Select(p => new AddressUnspentOutputModel
            {
                TransactionId = p.Id.TransactionId,
                AddressBalanceChangeModel = new AddressBalanceChangeModel
                {
                    AssetId = new AssetIdModel
                    {
                        Address = p.Asset.Address,
                        Ticker = p.Asset.Id
                    },
                    Address = p.Address,
                    Amount = p.Value.ToString(),
                    BlockId = p.BlockId,
                    BlockNumber = p.BlockNumber,
                    ConfirmationsCount = lastBlockNumber - p.BlockNumber,
                    //TODO
                    IsIrreversible = true
                }
            }).ToList();
        }
    }
}
