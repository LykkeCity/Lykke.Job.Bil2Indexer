using System.Collections.Generic;
using System.Linq;
using Lykke.Bil2.SharedDomain;
using Lykke.Numerics;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class AddressBalanceModelMapper
    {
        public static IReadOnlyCollection<AddressBalanceModel> ToViewModel(this IReadOnlyDictionary<Address, IReadOnlyDictionary<Asset, Money>> source)
        {
            return ToViewModelInner(source).ToList();
        }

        private static IEnumerable<AddressBalanceModel> ToViewModelInner(
            this IReadOnlyDictionary<Address, IReadOnlyDictionary<Asset, Money>> source)
        {
            foreach (var addrTuple in source)
            {
                foreach (var assetTuple in addrTuple.Value)
                {
                    yield return new AddressBalanceModel
                    {
                        Address = addrTuple.Key,
                        Amount = assetTuple.Value.ToString(),
                        AssetId = new AssetIdModel
                        {
                            Address = assetTuple.Key.Address,
                            Ticker = assetTuple.Key.Id
                        }
                    };
                }
            }
        }

        public static IReadOnlyCollection<AddressBalanceModel> ToViewModel(this IReadOnlyDictionary<Asset, Money> source, Address address)
        {
            return source.Select(p => new AddressBalanceModel
            {
                Address =  address,
                AssetId = new AssetIdModel
                {
                    Address = p.Key.Address,
                    Ticker = p.Key.Id
                },
                Amount = p.Value.ToString()
            }).ToList();
        }
    }
}
