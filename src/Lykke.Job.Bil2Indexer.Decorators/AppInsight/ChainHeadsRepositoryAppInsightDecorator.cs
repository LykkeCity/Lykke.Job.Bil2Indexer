using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services.Infrastructure;

namespace Lykke.Job.Bil2Indexer.Decorators.AppInsight
{
    [UsedImplicitly]
    public class ChainHeadsRepositoryAppInsightDecorator : IChainHeadsRepository
    {
        private readonly IChainHeadsRepository _impl;
        private readonly IAppInsightTelemetryProvider _appInsightTelemetryProvider;

        public ChainHeadsRepositoryAppInsightDecorator(IChainHeadsRepository chainHeadsRepository,
            IAppInsightTelemetryProvider appInsightTelemetryProvider)
        {
            _impl = chainHeadsRepository;
            _appInsightTelemetryProvider = appInsightTelemetryProvider;
        }

        public Task<ChainHead> GetOrDefaultAsync(string blockchainType)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _impl.GetOrDefaultAsync(blockchainType));
        }

        public Task<ChainHead> GetAsync(string blockchainType)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _impl.GetAsync(blockchainType));
        }

        public Task SaveAsync(ChainHead head)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{head.BlockId}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _impl.SaveAsync(head));
        }
    }
}
