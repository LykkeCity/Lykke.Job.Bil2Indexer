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
    public class FeeEnvelopesRepositoryAppInsightDecorator : IFeeEnvelopesRepository
    {
        private readonly IFeeEnvelopesRepository _impl;
        private readonly IAppInsightTelemetryProvider _appInsightTelemetryProvider;

        public FeeEnvelopesRepositoryAppInsightDecorator(IFeeEnvelopesRepository feeEnvelopesRepository,
            IAppInsightTelemetryProvider appInsightTelemetryProvider)
        {
            _impl = feeEnvelopesRepository;
            _appInsightTelemetryProvider = appInsightTelemetryProvider;
        }

        public async Task AddIfNotExistsAsync(IEnumerable<FeeEnvelope> fees)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{Guid.NewGuid()}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _impl.AddIfNotExistsAsync(fees));
        }

        public async Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{blockId}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _impl.TryRemoveAllOfBlockAsync(blockchainType, blockId));
        }

        public async Task<FeeEnvelope> GetOrDefaultAsync(string blockchainType, TransactionId transactionId, Asset asset)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{transactionId}-{asset.Id}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _impl.GetOrDefaultAsync(blockchainType, transactionId, asset));
        }

        public async Task<FeeEnvelope> GetAsync(string blockchainType, TransactionId transactionId, Asset asset)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{transactionId}-{asset.Id}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _impl.GetAsync(blockchainType, transactionId, asset));
        }

        public async Task<IReadOnlyCollection<FeeEnvelope>> GetTransactionFeesAsync(string blockchainType, TransactionId transactionId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{transactionId}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _impl.GetTransactionFeesAsync(blockchainType, transactionId));
        }

        public async Task<PaginatedItems<FeeEnvelope>> GetBlockFeesAsync(string blockchainType, BlockId blockId, long limit, string continuation)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{blockId}-{limit}-{continuation}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _impl.GetBlockFeesAsync(blockchainType, blockId, limit, continuation));
        }
    }
}
