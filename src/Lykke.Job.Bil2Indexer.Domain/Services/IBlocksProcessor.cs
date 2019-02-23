using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Services
{
    public interface IBlocksProcessor
    {
        Task ProcessBlockAsync(BlockHeader block);
    }
}
