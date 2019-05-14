using System.Collections.Generic;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class TransactionModelMapper
    {
        public static ActionResult<Paginated<TransactionModel>> Map(IReadOnlyCollection<Transaction> transactions)
        {
            throw new System.NotImplementedException();
        }

        public static ActionResult<TransactionModel> Map(Transaction transactions)
        {
            throw new System.NotImplementedException();
        }
    }
}
