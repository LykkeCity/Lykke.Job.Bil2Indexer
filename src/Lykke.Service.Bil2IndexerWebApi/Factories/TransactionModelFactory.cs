using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Factories
{
    public class TransactionModelFactory : ITransactionModelFactory
    {
        public TransactionModel PrepareTransactionModel(Transaction transaction)
        {
            throw new System.NotImplementedException();
        }

        public Paginated<TransactionModel[]> PrepareTransactionsPaginated(Transaction[] transactions)
        {
            throw new System.NotImplementedException();
        }
    }
}
