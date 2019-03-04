using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;

namespace Lykke.Job.Bil2Indexer.DomainServices
{
    public class BlocksBuilder : IBlocksBuilder
    {
        private readonly IBlockBuildingsRepository _blockBuildingsRepository;

        public BlocksBuilder(
            IBlockBuildingsRepository blockBuildingsRepository)
        {
            _blockBuildingsRepository = blockBuildingsRepository;
        }

        public Task AddHeader(BlockHeader blockHeader)
        {
            throw new System.NotImplementedException();
        }
    }
}
