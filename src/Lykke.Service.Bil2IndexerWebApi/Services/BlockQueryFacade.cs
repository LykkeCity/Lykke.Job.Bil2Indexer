using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Service.Bil2IndexerWebApi.Mappers;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public class BlockQueryFacade : IBlockQueryFacade
    {
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly IChainHeadsRepository _chainHeadsRepository;

        public BlockQueryFacade(IBlockHeadersRepository blockHeadersRepository,
            IChainHeadsRepository chainHeadsRepository)
        {
            _blockHeadersRepository = blockHeadersRepository;
            _chainHeadsRepository = chainHeadsRepository;
        }

        public async Task<BlockModel> GetBlockByIdOrDefault(string blockchainType, BlockId id)
        {
            var getBlock = _blockHeadersRepository.GetOrDefaultAsync(blockchainType, id);
            var getChainHead = _chainHeadsRepository.GetAsync(blockchainType);

            await Task.WhenAll(getBlock, getChainHead);
            if (getBlock.Result.Number <= getChainHead.Result.BlockNumber)
            {
                return getBlock.Result.ToViewModel();
            }

            return null;
        }

        public async Task<BlockModel> GetBlockByNumberOrDefault(string blockchainType, int number)
        {
            var getBlock = _blockHeadersRepository.GetOrDefaultAsync(blockchainType, number);
            var getChainHead = GetChainHeadNumberAsync(blockchainType);

            await Task.WhenAll(getBlock, getChainHead);
            if (getBlock.Result.Number <= getChainHead.Result)
            {
                return getBlock.Result.ToViewModel();
            }

            return null;
        }

        public async Task<IReadOnlyCollection<BlockModel>> GetBlocks(string blockchainType, int limit, bool orderAsc, string startingAfter,
            string endingBefore)
        {
            var getChainHead = GetChainHeadNumberAsync(blockchainType);
            var getBlocks = _blockHeadersRepository.GetAllAsync(blockchainType, limit, orderAsc, startingAfter, endingBefore);

            await Task.WhenAll(getBlocks, getChainHead);

            return getBlocks.Result.Where(p => p.Number <= getChainHead.Result).ToList().ToViewModel();
        }

        public Task<BlockModel> GetLastIrreversibleBlockAsync(string blockchainType)
        {
            //TODO discuss
            return GetLastBlockAsync(blockchainType);
        }

        public async Task<BlockModel> GetLastBlockAsync(string blockchainType)
        {
            var head = await _chainHeadsRepository.GetAsync(blockchainType);

            return (await _blockHeadersRepository.GetAsync(blockchainType, head.BlockId)).ToViewModel();
        }

        private async Task<long> GetChainHeadNumberAsync(string blockchainType)
        {
            return (await _chainHeadsRepository.GetAsync(blockchainType))?.BlockNumber ?? 0;
        }
    }
}
