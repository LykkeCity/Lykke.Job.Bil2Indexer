using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Factories
{
    public interface ITransactionModelFactory
    {
        TransactionModel PrepareTransactionModel(Transaction transaction);
        Paginated<TransactionModel[]> PrepareTransactionsPaginated(Transaction[] transactions);
    }
}
