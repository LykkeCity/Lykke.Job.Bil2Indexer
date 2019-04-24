using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services.Infrastructure;

namespace Lykke.Job.Bil2Indexer.Decorators.AppInsight
{
    [UsedImplicitly]
    public class BlockHeadersRepositoryAppInsightDecorator : IBlockHeadersRepository
    {
        private readonly IBlockHeadersRepository _impl;
        private readonly IAppInsightTelemetryProvider _appInsightTelemetryProvider;

        public BlockHeadersRepositoryAppInsightDecorator(IBlockHeadersRepository blockHeadersRepository,
            IAppInsightTelemetryProvider appInsightTelemetryProvider)
        {
            _impl = blockHeadersRepository;
            _appInsightTelemetryProvider = appInsightTelemetryProvider;
        }

        public async Task SaveAsync(BlockHeader block)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{block.Id}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _impl.SaveAsync(block));
        }

        public async Task<BlockHeader> GetOrDefaultAsync(string blockchainType, long blockNumber)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{blockNumber}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _impl.GetOrDefaultAsync(blockchainType, blockNumber));
        }

        public async Task<BlockHeader> GetOrDefaultAsync(string blockchainType, BlockId blockId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{blockId}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _impl.GetOrDefaultAsync(blockchainType, blockId));
        }

        public async Task<BlockHeader> GetAsync(string blockchainType, BlockId blockId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{blockId}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _impl.GetAsync(blockchainType, blockId));
        }

        public async Task TryRemoveAsync(string blockchainType, BlockId blockId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{blockId}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _impl.TryRemoveAsync(blockchainType, blockId));
        }
    }
}
