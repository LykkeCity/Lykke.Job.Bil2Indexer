using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Factories
{
    public interface IAddressModelFactory
    {
        Paginated<AddressBalanceModel[]> PrepareBalancesPaginated(Balance[] balances);
        Paginated<AddressUnspentOutputModel[]> PrepareUnspentOutputsPaginated(UnspentOutput[] unspentOutputs);
    }
}
