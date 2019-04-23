using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.Decorators
{
    public class ChainHeadsDecoratorRepository : IChainHeadsRepository
    {
        private IChainHeadsRepository _chainHeadsRepository;
        private IAppInsightTelemetryProvider _appInsightTelemetryProvider;

        public ChainHeadsDecoratorRepository(IChainHeadsRepository chainHeadsRepository,
            IAppInsightTelemetryProvider appInsightTelemetryProvider)
        {
            _chainHeadsRepository = chainHeadsRepository;
            _appInsightTelemetryProvider = appInsightTelemetryProvider;
        }

        public async Task<ChainHead> GetOrDefaultAsync(string blockchainType)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(ChainHeadsDecoratorRepository),
                nameof(GetOrDefaultAsync));
            var operationId = $"{blockchainType}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _chainHeadsRepository.GetOrDefaultAsync(blockchainType));
        }

        public async Task<ChainHead> GetAsync(string blockchainType)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(ChainHeadsDecoratorRepository),
                nameof(GetAsync));
            var operationId = $"{blockchainType}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _chainHeadsRepository.GetAsync(blockchainType));
        }

        public async Task SaveAsync(ChainHead head)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(ChainHeadsDecoratorRepository),
                nameof(SaveAsync));
            var operationId = $"{head.BlockId}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _chainHeadsRepository.SaveAsync(head));
        }
    }
}
