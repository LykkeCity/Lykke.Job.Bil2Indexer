using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.Decorators
{
    public class FeeEnvelopesDecoratorRepository : IFeeEnvelopesRepository
    {
        private IFeeEnvelopesRepository _feeEnvelopesRepository;
        private IAppInsightTelemetryProvider _appInsightTelemetryProvider;

        public FeeEnvelopesDecoratorRepository(IFeeEnvelopesRepository feeEnvelopesRepository,
            IAppInsightTelemetryProvider appInsightTelemetryProvider)
        {
            _feeEnvelopesRepository = feeEnvelopesRepository;
            _appInsightTelemetryProvider = appInsightTelemetryProvider;
        }

        public async Task AddIfNotExistsAsync(IReadOnlyCollection<FeeEnvelope> fees)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(FeeEnvelopesDecoratorRepository),
                nameof(AddIfNotExistsAsync));
            var operationId = $"{Guid.NewGuid()}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _feeEnvelopesRepository.AddIfNotExistsAsync(fees));
        }

        public async Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(FeeEnvelopesDecoratorRepository),
                nameof(TryRemoveAllOfBlockAsync));
            var operationId = $"{blockchainType}-{blockId}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _feeEnvelopesRepository.TryRemoveAllOfBlockAsync(blockchainType, blockId));
        }

        public async Task<FeeEnvelope> GetOrDefaultAsync(string blockchainType, TransactionId transactionId, Asset asset)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(FeeEnvelopesDecoratorRepository),
                nameof(GetOrDefaultAsync));
            var operationId = $"{blockchainType}-{transactionId}-{asset.Id}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _feeEnvelopesRepository.GetOrDefaultAsync(blockchainType, transactionId, asset));
        }

        public async Task<FeeEnvelope> GetAsync(string blockchainType, TransactionId transactionId, Asset asset)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(FeeEnvelopesDecoratorRepository),
                nameof(GetAsync));
            var operationId = $"{blockchainType}-{transactionId}-{asset.Id}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _feeEnvelopesRepository.GetAsync(blockchainType, transactionId, asset));
        }

        public async Task<IReadOnlyCollection<FeeEnvelope>> GetTransactionFeesAsync(string blockchainType, TransactionId transactionId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(FeeEnvelopesDecoratorRepository),
                nameof(GetTransactionFeesAsync));
            var operationId = $"{blockchainType}-{transactionId}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _feeEnvelopesRepository.GetTransactionFeesAsync(blockchainType, transactionId));
        }

        public async Task<PaginatedItems<FeeEnvelope>> GetBlockFeesAsync(string blockchainType, BlockId blockId, long limit, string continuation)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(FeeEnvelopesDecoratorRepository),
                nameof(GetBlockFeesAsync));
            var operationId = $"{blockchainType}-{blockId}-{limit}-{continuation}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _feeEnvelopesRepository.GetBlockFeesAsync(blockchainType, blockId, limit, continuation));
        }
    }
}
