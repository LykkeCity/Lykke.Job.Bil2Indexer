using System;
using System.Collections.Generic;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class AddressUnspentOutputModelMapper
    {
        public static IReadOnlyCollection<AddressUnspentOutputModel> ToViewModel(this IReadOnlyCollection<Coin> source)
        {
            throw new NotImplementedException();
        }
    }
}
