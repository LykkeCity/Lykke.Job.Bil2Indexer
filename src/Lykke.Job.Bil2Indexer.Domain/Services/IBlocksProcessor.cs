using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Services
{
    public interface IBlocksProcessor
    {
        Task StartAsync();
        Task ProcessBlockAsync(BlockHeader block);
    }
}
