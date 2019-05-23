using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Service.Bil2IndexerWebApi.Mappers;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public class AddressQueryFacade: IAddressQueryFacade
    {
        private readonly ICoinsRepository _coinsRepository;
        private readonly IChainHeadsRepository _chainHeadsRepository;
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly IBalanceActionsRepository _balanceActionsRepository;

        public AddressQueryFacade(ICoinsRepository coinsRepository, 
            IChainHeadsRepository chainHeadsRepository, 
            IBlockHeadersRepository blockHeadersRepository, 
            IBalanceActionsRepository balanceActionsRepository)
        {
            _coinsRepository = coinsRepository;
            _chainHeadsRepository = chainHeadsRepository;
            _blockHeadersRepository = blockHeadersRepository;
            _balanceActionsRepository = balanceActionsRepository;
        }

        public async Task<IReadOnlyCollection<AddressUnspentOutputResponce>> GetUnspentOutputs(string blockchainType,
            string address, 
            int limit, 
            bool orderAsc,
            string startingAfter,
            string endingBefore)
        {
            var getChainHead = _chainHeadsRepository.GetChainHeadNumberAsync(blockchainType);

            return (await _coinsRepository.GetUnspentAsync(blockchainType, 
                        address, 
                        limit, 
                        orderAsc,
                        CoinIdBuilder.BuildDomainOrDefault(startingAfter), 
                        CoinIdBuilder.BuildDomainOrDefault(endingBefore)))
                .ToViewModel(await getChainHead);
        }

        public async Task<IReadOnlyCollection<AddressBalanceResponce>> GetBalances(string blockchainType, 
            string address)
        {
            var chainHead = await _chainHeadsRepository.GetAsync(blockchainType);
            var block = await _blockHeadersRepository.GetAsync(blockchainType, chainHead.BlockId);

            return await GetBalancesInner(blockchainType, address, block);
        }

        public async Task<IReadOnlyCollection<AddressBalanceResponce>> GetBalancesByBlockId(string blockchainType,
            string address,
            string blockId)
        {
            var block = await _blockHeadersRepository.GetAsync(blockchainType, new BlockId(blockId));

            return await GetBalancesInner(blockchainType, address, block);
        }

        public async Task<IReadOnlyCollection<AddressBalanceResponce>> GetBalancesByBlockNumber(string blockchainType, 
            string address,
            long blockNumber)
        {
            var block = await _blockHeadersRepository.GetAsync(blockchainType, blockNumber);

            return await GetBalancesInner(blockchainType, address, block);
        }
        

        public async Task<IReadOnlyCollection<AddressBalanceResponce>> GetBalancesOnDate(string blockchainType,
            string address, 
            DateTime date)
        {
            var block = await _blockHeadersRepository.GetAsync(blockchainType, date);
            
            return await GetBalancesInner(blockchainType, address, block);
        }

        private async Task<IReadOnlyCollection<AddressBalanceResponce>> GetBalancesInner(string blockchainType,
            string address,
            BlockHeader blockHeader)
        {
            var balances = await _balanceActionsRepository.GetBalancesAsync(blockchainType, address, blockHeader.Number);
            
            //TODO align with chainhead
            return balances.ToViewModel(address, blockHeader);
        }
    }
}
