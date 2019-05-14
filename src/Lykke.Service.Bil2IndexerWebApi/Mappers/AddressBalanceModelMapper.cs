using System.Collections.Generic;
using Lykke.Bil2.SharedDomain;
using Lykke.Numerics;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class AddressBalanceModelMapper
    {
        public static ActionResult<Paginated<AddressBalanceModel>> Map(IReadOnlyDictionary<Address, IReadOnlyDictionary<Asset, Money>> balances)
        {
            throw new System.NotImplementedException();
        }

        public static ActionResult<Paginated<AddressBalanceModel>> Map(IReadOnlyDictionary<Asset, Money> balances)
        {
            throw new System.NotImplementedException();
        }
    }
}
