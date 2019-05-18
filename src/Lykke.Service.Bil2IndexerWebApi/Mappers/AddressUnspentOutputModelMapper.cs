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
                AddressBalanceModel = new AddressBalanceModel
                {
                    AssetId = new AssetIdModel
                    {
                        Address = p.Asset.Address,
                        Ticker = p.Asset.Id
                    },
                    Address = p.Address,
                    Amount = p.Value.ToString(),

                    //TODO
                    BlockId = null,
                    IsIrreversible =true,
                    BlockNumber = -1,
                    ConfirmationsCount = -1
                }
            }).ToList();
        }
    }
}
