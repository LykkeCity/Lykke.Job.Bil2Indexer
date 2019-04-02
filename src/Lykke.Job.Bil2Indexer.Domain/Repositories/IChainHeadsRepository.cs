using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface IChainHeadsRepository
    {
        Task<ChainHead> GetOrDefaultAsync(string blockchainType);
        Task<ChainHead> GetAsync(string blockchainType);
        Task SaveAsync(ChainHead head);
    }
}
