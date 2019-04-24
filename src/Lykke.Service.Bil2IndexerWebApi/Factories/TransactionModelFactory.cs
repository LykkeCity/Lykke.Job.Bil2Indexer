using DataApi.Core.Domain;
using DataApi.Models;
using DataApi.Models.Common;

namespace DataApi.Factories
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
