using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface IBlockExpectationRepository
    {
        Task<BlockExpectation> GetOrDefaultAsync(string crawlerId);
        Task SaveAsync(string crawlerId, BlockExpectation blockExpectation);
    }
}
