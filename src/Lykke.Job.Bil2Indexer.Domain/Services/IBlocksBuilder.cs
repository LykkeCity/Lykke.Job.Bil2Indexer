using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Services
{
    public interface IBlocksBuilder
    {
        Task AddHeader(BlockHeader blockHeader);
    }
}
