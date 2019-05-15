using System;
using System.Collections.Generic;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class AddressUnspentOutputModelMapper
    {
        public static Paginated<AddressUnspentOutputModel> ToViewModel(this IReadOnlyCollection<Coin> unspentOutputs)
        {
            throw new NotImplementedException();
        }
    }
}
