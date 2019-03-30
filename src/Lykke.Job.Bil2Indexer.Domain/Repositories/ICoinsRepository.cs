using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.Contract.Common;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface ICoinsRepository
    {
        Task SaveAsync(string blockchainType, string transactionId, IEnumerable<ReceivedCoin> coins);
        Task<ReceivedCoin> GetToSpendAsync(string blockchainType, CoinReference reference, string toSpendByTransactionId);
        Task SpendAsync(string blockchainType, CoinReference reference, string byTransactionId);
    }
}
