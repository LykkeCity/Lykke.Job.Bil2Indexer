using System.Collections.Generic;
using System.Linq;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class TransactionModelMapper
    {
        public static IReadOnlyCollection<TransactionModel> ToViewModel(this IReadOnlyCollection<Transaction> source, 
            IReadOnlyCollection<FeeEnvelope> fees,
            IReadOnlyCollection<BalanceAction> balances)
        {
            var feesPerTx = fees.ToLookup(p => p.TransactionId);
            var balancesPerTx = balances.ToLookup(p => p.TransactionId);
            
            throw new System.NotImplementedException();
        }

        public static TransactionModel ToViewModel(this Transaction source,
            IReadOnlyCollection<FeeEnvelope> fees,
            IReadOnlyCollection<BalanceAction> balances)
        {
            
            return new TransactionModel
            {
                Id = source.
            }
        }
    }
}
