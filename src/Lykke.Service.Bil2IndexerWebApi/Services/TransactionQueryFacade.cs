using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Service.Bil2IndexerWebApi.Mappers;
using Lykke.Service.Bil2IndexerWebApi.Models;

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

        public async Task<TransactionModel> GetTransactionById(string blockchainType, string id)
        {
            return (await GetTransactions(blockchainType, new List<TransactionId> {new TransactionId(id)}))
                .SingleOrDefault();
        }

        public async Task<IReadOnlyCollection<TransactionModel>> GetTransactionsByBlockId(string blockchainType, 
            string blockId,
            int limit, 
            bool orderAsc,
            string startingAfter,
            string endingBefore)
        {
            var transactionIds = await _balanceActionsRepository.GetTransactionsOfBlockAsync(blockchainType,
                new BlockId(blockId),
                limit,
                orderAsc, 
                startingAfter, 
                endingBefore);

            return await GetTransactions(blockchainType, transactionIds);
        }

        public async Task<IReadOnlyCollection<TransactionModel>> GetTransactionsByAddress(string blockchainType, 
            string address,
            int limit,
            bool orderAsc,
            string startingAfter,
            string endingBefore)
        {
            var transactionIds = await _balanceActionsRepository.GetTransactionsOfAddressAsync(blockchainType,
                new Address(address), 
                limit,
                orderAsc,
                startingAfter,
                endingBefore);

            return await GetTransactions(blockchainType, transactionIds);
        }

        private async Task<IReadOnlyCollection<TransactionModel>> GetTransactions(string blockchainType,
            IReadOnlyCollection<TransactionId> transactionIds)
        {
            var getLastBlockNumber = _chainHeadsRepository.GetChainHeadNumberAsync(blockchainType);

            var getBalanceActions = _balanceActionsRepository.GetCollectionAsync(blockchainType,
                transactionIds.ToArray());

            var getFees = _feeEnvelopesRepository.GetCollectionsAsync(blockchainType, transactionIds.ToList());

            await Task.WhenAll(getLastBlockNumber, getBalanceActions, getFees);

            //TODO check with chain head
            return transactionIds.ToViewModel(getFees.Result, 
                getBalanceActions.Result,
                getLastBlockNumber.Result);
        }

        public async Task<IReadOnlyCollection<TransactionModel>> GetTransactionsByBlockNumber(string blockchainType, 
            int blockNumberValue,
            int limit,
            bool orderAsc,
            string startingAfter,
            string endingBefore)
        {
            var block = await _blockHeadersRepository.GetOrDefaultAsync(blockchainType, blockNumberValue);

            if (block == null)
            {
                return Enumerable.Empty<TransactionModel>().ToList();
            }

            return await GetTransactionsByBlockId(blockchainType,
                block.Id,
                limit,
                orderAsc,
                startingAfter,
                endingBefore);
        }
    }
}
