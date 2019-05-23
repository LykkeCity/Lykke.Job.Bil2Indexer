using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Service.Bil2IndexerWebApi.Mappers;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    internal class BlockQueryFacade : IBlockQueryFacade
    {
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly IChainHeadsRepository _chainHeadsRepository;

        public BlockQueryFacade(IBlockHeadersRepository blockHeadersRepository,
            IChainHeadsRepository chainHeadsRepository)
        {
            _blockHeadersRepository = blockHeadersRepository;
            _chainHeadsRepository = chainHeadsRepository;
        }

        public async Task<BlockResponce> GetBlockByIdOrDefault(string blockchainType, BlockId id, IUrlHelper url)
        {
            var getBlock = _blockHeadersRepository.GetOrDefaultAsync(blockchainType, id);
            var getChainHead = _chainHeadsRepository.GetChainHeadNumberAsync(blockchainType);

            await Task.WhenAll(getBlock, getChainHead);
            
            if (getBlock.Result?.Number <= getChainHead.Result)
            {
                return getBlock.Result.ToViewModel(getChainHead.Result, url, blockchainType);
            }

            return null;
        }

        public async Task<BlockResponce> GetBlockByNumberOrDefault(string blockchainType, long number, IUrlHelper url)
        {
            var getBlock = _blockHeadersRepository.GetOrDefaultAsync(blockchainType, number);
            var getChainHead = _chainHeadsRepository.GetChainHeadNumberAsync(blockchainType);

            await Task.WhenAll(getBlock, getChainHead);
            if (getBlock.Result?.Number <= getChainHead.Result)
            {
                return getBlock.Result.ToViewModel(getChainHead.Result, url, blockchainType);
            }

            return null;
        }

        public async Task<IReadOnlyCollection<BlockResponce>> GetBlocks(string blockchainType, 
            int limit, 
            bool orderAsc, 
            long? startingAfterNumber,
            long? endingBeforeNumber,
            IUrlHelper url)
        {
            var chainHead = await _chainHeadsRepository.GetChainHeadNumberAsync(blockchainType);

            var result =await _blockHeadersRepository
                .GetCollectionAsync(blockchainType, chainHead, limit, orderAsc, startingAfterNumber, endingBeforeNumber);

            return result.ToViewModel(chainHead, url, blockchainType);
        }

        public Task<BlockResponce> GetLastIrreversibleBlockAsync(string blockchainType, IUrlHelper url)
        {
            //TODO implement using new repo
            return GetLastBlockAsync(blockchainType, url);
        }

        public async Task<BlockResponce> GetLastBlockAsync(string blockchainType, IUrlHelper url)
        {
            var head = await _chainHeadsRepository.GetAsync(blockchainType);

            return (await _blockHeadersRepository.GetAsync(blockchainType, head.BlockId))
                .ToViewModel(head.BlockNumber ?? 0, url, blockchainType);
        }
    }
}
