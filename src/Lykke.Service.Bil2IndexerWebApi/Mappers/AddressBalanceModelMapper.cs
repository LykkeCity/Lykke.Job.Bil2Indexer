using System.Collections.Generic;
using Lykke.Bil2.SharedDomain;
using Lykke.Numerics;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class AddressBalanceModelMapper
    {
        public static Paginated<AddressBalanceModel> ToViewModel(this IReadOnlyDictionary<Address, IReadOnlyDictionary<Asset, Money>> balances)
        {
            throw new System.NotImplementedException();
        }

        public static Paginated<AddressBalanceModel> ToViewModel(this IReadOnlyDictionary<Asset, Money> balances)
        {
            throw new System.NotImplementedException();
        }
    }
}
