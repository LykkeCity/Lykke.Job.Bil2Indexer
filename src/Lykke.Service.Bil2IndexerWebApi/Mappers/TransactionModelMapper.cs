using System.Collections.Generic;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class TransactionModelMapper
    {
        public static IReadOnlyCollection<TransactionModel> ToViewModel(this IReadOnlyCollection<Transaction> transactions)
        {
            throw new System.NotImplementedException();
        }

        public static TransactionModel ToViewModel(this Transaction transactions)
        {
            throw new System.NotImplementedException();
        }
    }
}
