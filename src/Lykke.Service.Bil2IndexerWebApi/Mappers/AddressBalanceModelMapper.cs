using System.Collections.Generic;
using Lykke.Bil2.SharedDomain;
using Lykke.Numerics;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class AddressBalanceModelMapper
    {
        public static IReadOnlyCollection<AddressBalanceModel> ToViewModel(this IReadOnlyDictionary<Address, IReadOnlyDictionary<Asset, Money>> source)
        {
            throw new System.NotImplementedException();
        }

        public static IReadOnlyCollection<AddressBalanceModel> ToViewModel(this IReadOnlyDictionary<Asset, Money> source)
        {

            throw new System.NotImplementedException();
        }
    }
}
