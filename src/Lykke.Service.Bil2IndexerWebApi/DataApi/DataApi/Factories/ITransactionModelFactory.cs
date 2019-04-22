using DataApi.Core.Domain;
using DataApi.Models;
using DataApi.Models.Common;

namespace DataApi.Factories
{
    public interface ITransactionModelFactory
    {
        TransactionModel PrepareTransactionModel(Transaction transaction);
        Paginated<TransactionModel[]> PrepareTransactionsPaginated(Transaction[] transactions);
    }
}
