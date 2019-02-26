using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface IBlockExpectationRepository
    {
        Task<BlockExpectation> GetOrDefaultAsync();
        Task SaveAsync(BlockExpectation blockExpectation);
    }
}
