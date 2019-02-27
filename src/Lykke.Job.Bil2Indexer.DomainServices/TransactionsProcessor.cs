using System;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;

namespace Lykke.Job.Bil2Indexer.DomainServices
{
    public class TransactionsProcessor : ITransactionsProcessor
    {
        private readonly ITransactionsRepository _transactionsRepository;

        public TransactionsProcessor(ITransactionsRepository transactionsRepository)
        {
            _transactionsRepository = transactionsRepository;
        }

        public async Task ProcessExecutedTransactionAsync(TransactionExecutedEvent transaction)
        {
            await _transactionsRepository.SaveAsync(transaction);
        }

        public async Task ProcessFailedTransactionAsync(TransactionFailedEvent transaction)
        {
            await _transactionsRepository.SaveAsync(transaction);
        }
    }
}
