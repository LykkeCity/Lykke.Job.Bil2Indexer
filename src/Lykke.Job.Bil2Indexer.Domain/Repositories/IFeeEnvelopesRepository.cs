using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface IFeeEnvelopesRepository
    {
        Task SaveAsync(IEnumerable<FeeEnvelope> fees);
        Task<FeeEnvelope> GetOrDefaultAsync(string blockchainType, string transactionId, Asset asset);
        Task<FeeEnvelope> GetAsync(string blockchainType, string transactionId, Asset asset);
        Task<PaginatedItems<FeeEnvelope>> GetTransactionFeesAsync(string blockchainType, string transactionId, long limit, string continuation);
        Task<PaginatedItems<FeeEnvelope>> GetBlockFeesAsync(string blockchainType, string blockId, long limit, string continuation);
        Task TryRemoveAllOfBlockAsync(string blockchainType, string blockId);
    }
}
