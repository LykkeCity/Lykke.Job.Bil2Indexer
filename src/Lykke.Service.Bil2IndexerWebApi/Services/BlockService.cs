using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public class BlockService : IBlockService
    {
        private readonly IBlockHeadersRepository _blockHeadersRepository;

        public BlockService(IBlockHeadersRepository blockHeadersRepository)
        {
            _blockHeadersRepository = blockHeadersRepository;
        }

        public async Task<BlockHeader> GetBlockByIdOrDefault(BlockId id)
        {
            var block = await _blockHeadersRepository.GetOrDefaultAsync(id);

            if (block.) //assembled | executed depending on transfer model
            {
                return block;
            }

            return null;
        }

        public Task<BlockHeader> GetBlockByNumberOrDefault(int number)
        {
            throw new System.NotImplementedException();
        }

        public Task<BlockHeader[]> GetBlocks(int limit, bool orderAsc, string startingAfter, string endingBefore)
        {
            throw new System.NotImplementedException();
        }
    }
}
