using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface IFeeEnvelopesRepository
    {
        Task AddIfNotExistsAsync(IReadOnlyCollection<FeeEnvelope> fees);
        Task<FeeEnvelope> GetOrDefaultAsync(string blockchainType, TransactionId transactionId, Asset asset);
        Task<FeeEnvelope> GetAsync(string blockchainType, TransactionId transactionId, Asset asset);
        Task<IReadOnlyCollection<FeeEnvelope>> GetTransactionFeesAsync(string blockchainType, TransactionId transactionId);
        Task<PaginatedItems<FeeEnvelope>> GetBlockFeesAsync(string blockchainType, BlockId blockId, long limit, string continuation);
        Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId);
    }
}
