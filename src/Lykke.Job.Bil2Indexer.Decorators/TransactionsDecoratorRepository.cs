using System;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.Decorators
{
    public class TransactionsDecoratorRepository : ITransactionsRepository
    {
        private IAppInsightTelemetryProvider _appInsightTelemetryProvider;
        private ITransactionsRepository _transactionsRepository;

        public TransactionsDecoratorRepository(ITransactionsRepository transactionsRepository,
            IAppInsightTelemetryProvider appInsightTelemetryProvider)
        {
            _transactionsRepository = transactionsRepository;
            _appInsightTelemetryProvider = appInsightTelemetryProvider;
        }

        public async Task AddIfNotExistsAsync(string blockchainType, TransferAmountTransactionExecutedEvent transaction)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(TransactionsDecoratorRepository),
                nameof(AddIfNotExistsAsync));
            var operationId = $"{blockchainType}-{transaction.TransactionId}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _transactionsRepository.AddIfNotExistsAsync(blockchainType, transaction));
        }

        public async Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(TransactionsDecoratorRepository),
                nameof(TryRemoveAllOfBlockAsync));
            var operationId = $"{blockchainType}-{blockId}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _transactionsRepository.TryRemoveAllOfBlockAsync(blockchainType, blockId));
        }

        public async Task AddIfNotExistsAsync(string blockchainType, TransferCoinsTransactionExecutedEvent transaction)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(TransactionsDecoratorRepository),
                nameof(AddIfNotExistsAsync));
            var operationId = $"{blockchainType}-{transaction.TransactionId}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _transactionsRepository.AddIfNotExistsAsync(blockchainType, transaction));
        }

        public async Task AddIfNotExistsAsync(string blockchainType, TransactionFailedEvent transaction)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(TransactionsDecoratorRepository),
                nameof(AddIfNotExistsAsync));
            var operationId = $"{blockchainType}-{transaction.TransactionId}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _transactionsRepository.AddIfNotExistsAsync(blockchainType, transaction));
        }

        public async Task<int> CountInBlockAsync(string blockchainType, BlockId blockId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(FeeEnvelopesDecoratorRepository),
                nameof(CountInBlockAsync));
            var operationId = $"{blockchainType}-{blockId}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _transactionsRepository.CountInBlockAsync(blockchainType,blockId));
        }

        public async Task<PaginatedItems<TransactionEnvelope>> GetAllOfBlockAsync(string blockchainType, BlockId blockId, int limit, string continuation)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(FeeEnvelopesDecoratorRepository),
                nameof(GetAllOfBlockAsync));
            var operationId = $"{blockchainType}-{blockId}-{limit}-{continuation}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _transactionsRepository.GetAllOfBlockAsync(blockchainType, blockId, limit, continuation));
        }

        public async Task<TransactionEnvelope> GetAsync(string blockchainType, TransactionId transactionId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(FeeEnvelopesDecoratorRepository),
                nameof(GetAsync));
            var operationId = $"{blockchainType}-{transactionId}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _transactionsRepository.GetAsync(blockchainType, transactionId));
        }

        public async Task<TransactionEnvelope> GetOrDefaultAsync(string blockchainType, TransactionId transactionId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(FeeEnvelopesDecoratorRepository),
                nameof(GetOrDefaultAsync));
            var operationId = $"{blockchainType}-{transactionId}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _transactionsRepository.GetOrDefaultAsync(blockchainType, transactionId));
        }
    }
}
