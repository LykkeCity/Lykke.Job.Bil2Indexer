using System;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;

namespace Lykke.Job.Bil2Indexer.DomainServices
{
    public class TransactionsHandler : ITransactionsHandler
    {
        private readonly ITransactionsRepository _transactionsRepository;

        public TransactionsHandler(ITransactionsRepository transactionsRepository)
        {
            _transactionsRepository = transactionsRepository;
        }

        public Task ProcessExecutedTransactionAsync(TransferAmountTransactionExecutedEvent transaction)
        {
            return _transactionsRepository.SaveAsync(transaction);
        }

        public Task ProcessExecutedTransactionAsync(TransferCoinsTransactionExecutedEvent transaction)
        {
            return _transactionsRepository.SaveAsync(transaction);
        }

        public async Task ProcessFailedTransactionAsync(TransactionFailedEvent transaction)
        {
            await _transactionsRepository.SaveAsync(transaction);
        }
    }
}
