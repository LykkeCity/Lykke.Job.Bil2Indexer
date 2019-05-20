using System.Collections.Generic;
using System.Linq;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Numerics;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class AddressBalanceModelMapper
    {
        public static IReadOnlyCollection<AddressBalanceModel> ToViewModel(this IReadOnlyDictionary<Address, IReadOnlyDictionary<Asset, Money>> source, 
            BlockHeader blockHeader)
        {
            return source.SelectMany(p => ToViewModel(p.Value, p.Key, blockHeader)).ToList();
        }

        public static IReadOnlyCollection<AddressBalanceModel> ToViewModel(this IReadOnlyDictionary<Asset, Money> source, 
            Address address, 
            BlockHeader blockHeader)
        {
            return source.Select(p => new AddressBalanceModel
            {
                Address =  address,
                AssetId = new AssetIdModel
                {
                    Address = p.Key.Address,
                    Ticker = p.Key.Id
                },
                Amount = p.Value.ToString(),
                BlockId = blockHeader?.Id,
                BlockNumber = blockHeader?.Number
            }).ToList();
        }
    }
}
