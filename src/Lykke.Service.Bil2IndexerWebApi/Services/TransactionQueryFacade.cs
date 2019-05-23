using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Service.Bil2IndexerWebApi.Mappers;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public class TransactionQueryFacade: ITransactionQueryFacade
    {
        private readonly IFeeEnvelopesRepository _feeEnvelopesRepository;
        private readonly IBalanceActionsRepository _balanceActionsRepository;
        private readonly IChainHeadsRepository _chainHeadsRepository;
        private readonly IBlockHeadersRepository _blockHeadersRepository;

        public TransactionQueryFacade(IFeeEnvelopesRepository feeEnvelopesRepository, 
            IBalanceActionsRepository balanceActionsRepository, 
            IChainHeadsRepository chainHeadsRepository,
            IBlockHeadersRepository blockHeadersRepository)
        {
            _feeEnvelopesRepository = feeEnvelopesRepository;
            _balanceActionsRepository = balanceActionsRepository;
            _chainHeadsRepository = chainHeadsRepository;
            _blockHeadersRepository = blockHeadersRepository;
        }

        public async Task<TransactionResponce> GetTransactionById(string blockchainType, 
            string id,
            IUrlHelper url)
        {
            return (await GetTransactions(blockchainType, 
                    new List<TransactionId> {new TransactionId(id)}, 
                    url))
                .SingleOrDefault();
        }

        public async Task<IReadOnlyCollection<TransactionResponce>> GetTransactionsByBlockId(string blockchainType, 
            string blockId,
            int limit, 
            bool orderAsc,
            string startingAfter,
            string endingBefore,
            IUrlHelper url)
        {
            var getTransactionIds = _balanceActionsRepository.GetTransactionsOfBlockAsync(blockchainType,
                new BlockId(blockId),
                limit,
                orderAsc, 
                startingAfter, 
                endingBefore);

            var getChainHeadNumber = _chainHeadsRepository.GetChainHeadNumberAsync(blockchainType);
            var getBlockHeader = _blockHeadersRepository.GetOrDefaultAsync(blockchainType, blockId);

            await Task.WhenAll(getTransactionIds, getChainHeadNumber, getBlockHeader);

            if (getChainHeadNumber.Result >= (getBlockHeader.Result?.Number ?? long.MaxValue))
            {
                return await GetTransactions(blockchainType, getTransactionIds.Result, url);

            }

            //chain head not moved to block yet
            return Enumerable.Empty<TransactionResponce>().ToList();
        }

        public async Task<IReadOnlyCollection<TransactionResponce>> GetTransactionsByAddress(string blockchainType, 
            string address,
            int limit,
            bool orderAsc,
            string startingAfter,
            string endingBefore,
            IUrlHelper url)
        {
            var chainHeadNumber = await _chainHeadsRepository.GetChainHeadNumberAsync(blockchainType);
            var transactionIds = await _balanceActionsRepository.GetTransactionsOfAddressAsync(blockchainType,
                new Address(address), 
                chainHeadNumber,
                limit,
                orderAsc,
                startingAfter,
                endingBefore);

            return await GetTransactions(blockchainType, transactionIds, url);
        }

        private async Task<IReadOnlyCollection<TransactionResponce>> GetTransactions(string blockchainType,
            IReadOnlyCollection<TransactionId> transactionIds, IUrlHelper url, long? chainHeadNumber = null)
        {
            var getChainHeadNumber = chainHeadNumber != null ?
                Task.FromResult(chainHeadNumber.Value) :_chainHeadsRepository.GetChainHeadNumberAsync(blockchainType);

            var getBalanceActions = _balanceActionsRepository.GetCollectionAsync(blockchainType,
                transactionIds.ToArray());

            var getFees = _feeEnvelopesRepository.GetTransactionFeesAsync(blockchainType, transactionIds.ToList());

            await Task.WhenAll(getChainHeadNumber, getBalanceActions, getFees);
            
            //checked with chain head inside mapper
            return transactionIds.ToViewModel(getFees.Result, 
                getBalanceActions.Result,
                getChainHeadNumber.Result,
                url,
                blockchainType);
        }

        public async Task<IReadOnlyCollection<TransactionResponce>> GetTransactionsByBlockNumber(string blockchainType, 
            int blockNumberValue,
            int limit,
            bool orderAsc,
            string startingAfter,
            string endingBefore,
            IUrlHelper url)
        {

            var getBlock = _blockHeadersRepository.GetOrDefaultAsync(blockchainType, blockNumberValue);
            var getChainHeadNumber = _chainHeadsRepository.GetChainHeadNumberAsync(blockchainType);


            await Task.WhenAll(getChainHeadNumber, getBlock);

            if (getBlock.Result == null || getChainHeadNumber.Result < getBlock.Result.Number)
            {
                return Enumerable.Empty<TransactionResponce>().ToList();
            }

            return await GetTransactionsByBlockId(blockchainType,
                getBlock.Result.Id,
                limit,
                orderAsc,
                startingAfter,
                endingBefore,
                url);
        }
    }
}
