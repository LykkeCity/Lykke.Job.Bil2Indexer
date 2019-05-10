using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services.Infrastructure;

namespace Lykke.Job.Bil2Indexer.Decorators.AppInsight
{
    [UsedImplicitly]
    public class TransactionsRepositoryAppInsightDecorator : ITransactionsRepository
    {
        private readonly IAppInsightTelemetryProvider _appInsightTelemetryProvider;
        private readonly ITransactionsRepository _impl;

        public TransactionsRepositoryAppInsightDecorator(ITransactionsRepository transactionsRepository,
            IAppInsightTelemetryProvider appInsightTelemetryProvider)
        {
            _impl = transactionsRepository;
            _appInsightTelemetryProvider = appInsightTelemetryProvider;
        }

        public Task AddIfNotExistsAsync(IReadOnlyCollection<Transaction> transactions)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = Guid.NewGuid().ToString();

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _impl.AddIfNotExistsAsync(transactions));
        }

        public Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{blockId}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _impl.TryRemoveAllOfBlockAsync(blockchainType, blockId));
        }

        public Task<int> CountInBlockAsync(string blockchainType, BlockId blockId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{blockId}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _impl.CountInBlockAsync(blockchainType,blockId));
        }

        public Task<PaginatedItems<Transaction>> GetAllOfBlockAsync(string blockchainType, BlockId blockId, int limit, string continuation)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{blockId}-{limit}-{continuation}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _impl.GetAllOfBlockAsync(blockchainType, blockId, limit, continuation));
        }

        public Task<Transaction> GetAsync(string blockchainType, TransactionId transactionId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{transactionId}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _impl.GetAsync(blockchainType, transactionId));
        }

        public Task<Transaction> GetOrDefaultAsync(string blockchainType, TransactionId transactionId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{transactionId}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _impl.GetOrDefaultAsync(blockchainType, transactionId));
        }
    }
}
