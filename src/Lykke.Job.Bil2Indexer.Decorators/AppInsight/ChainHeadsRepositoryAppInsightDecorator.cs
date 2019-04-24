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
        private readonly IChainHeadsRepository _chainHeadsRepository;
        private readonly IAppInsightTelemetryProvider _appInsightTelemetryProvider;

        public ChainHeadsRepositoryAppInsightDecorator(IChainHeadsRepository chainHeadsRepository,
            IAppInsightTelemetryProvider appInsightTelemetryProvider)
        {
            _chainHeadsRepository = chainHeadsRepository;
            _appInsightTelemetryProvider = appInsightTelemetryProvider;
        }

        public async Task<ChainHead> GetOrDefaultAsync(string blockchainType)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_chainHeadsRepository);
            var operationId = $"{blockchainType}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _chainHeadsRepository.GetOrDefaultAsync(blockchainType));
        }

        public async Task<ChainHead> GetAsync(string blockchainType)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_chainHeadsRepository);
            var operationId = $"{blockchainType}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _chainHeadsRepository.GetAsync(blockchainType));
        }

        public async Task SaveAsync(ChainHead head)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_chainHeadsRepository);
            var operationId = $"{head.BlockId}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _chainHeadsRepository.SaveAsync(head));
        }
    }
}
