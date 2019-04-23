using DataApi.Core.Domain;
using DataApi.Models;
using DataApi.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Factories
{
    public interface IAddressModelFactory
    {
        Paginated<BalanceModel[]> PrepareBalancesPaginated(Balance[] balances);
        Paginated<UnspentOutputModel[]> PrepareUnspentOutputsPaginated(UnspentOutput[] unspentOutputs);
    }
}
