using System.Collections.Generic;
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

        public async Task<BlockHeader> GetBlockByIdOrDefault(string blockchainType, BlockId id)
        {
            var block = await _blockHeadersRepository.GetOrDefaultAsync(blockchainType, id);

            //if (block./*assembled | executed depending on transfer model*/) 
            //{
            //    return block;
            //}

            return null;
        }

        public Task<BlockHeader> GetBlockByNumberOrDefault(string blockchainType, int number)
        {
            throw new System.NotImplementedException();
        }

        public Task<IReadOnlyCollection<BlockHeader>> GetBlocks(string blockchainType, int limit, bool orderAsc, string startingAfter,
            string endingBefore)
        {
            throw new System.NotImplementedException();
        }

        public Task<BlockHeader> GetLastIrreversibleBlockAsync(string blockchainType)
        {
            throw new System.NotImplementedException();
        }

        public Task<BlockHeader> GetLastBlockAsync(string blockchainType)
        {
            throw new System.NotImplementedException();
        }
    }
}
