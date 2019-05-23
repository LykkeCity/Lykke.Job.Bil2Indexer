using System.Collections.Generic;
using System.Linq;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class AddressUnspentOutputModelMapper
    {
        public static IReadOnlyCollection<AddressUnspentOutputResponce> ToViewModel(this IReadOnlyCollection<Coin> source, long lastBlockNumber)
        {
            return source.Select(p => new AddressUnspentOutputResponce
            {
                Id = p.Id.BuildCoinId(),
                TransactionId = p.Id.TransactionId,
                AddressBalanceChangeResponce = new AddressBalanceChangeResponce
                {
                    AssetId = new AssetIdResponce
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
