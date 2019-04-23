using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.Decorators
{
    public class BlockHeadersDecoratorRepository : IBlockHeadersRepository
    {
        private IBlockHeadersRepository _blockHeadersRepository;
        private IAppInsightTelemetryProvider _appInsightTelemetryProvider;

        public BlockHeadersDecoratorRepository(IBlockHeadersRepository blockHeadersRepository,
            IAppInsightTelemetryProvider appInsightTelemetryProvider)
        {
            _blockHeadersRepository = blockHeadersRepository;
            _appInsightTelemetryProvider = appInsightTelemetryProvider;
        }

        public async Task SaveAsync(BlockHeader block)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(BlockHeadersDecoratorRepository),
                nameof(SaveAsync));
            var operationId = $"{block.Id}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _blockHeadersRepository.SaveAsync(block));
        }

        public async Task<BlockHeader> GetOrDefaultAsync(string blockchainType, long blockNumber)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(BlockHeadersDecoratorRepository),
                nameof(GetOrDefaultAsync));
            var operationId = $"{blockchainType}-{blockNumber}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _blockHeadersRepository.GetOrDefaultAsync(blockchainType, blockNumber));
        }

        public async Task<BlockHeader> GetOrDefaultAsync(string blockchainType, BlockId blockId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(BlockHeadersDecoratorRepository),
                nameof(GetOrDefaultAsync));
            var operationId = $"{blockchainType}-{blockId}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _blockHeadersRepository.GetOrDefaultAsync(blockchainType, blockId));
        }

        public async Task<BlockHeader> GetAsync(string blockchainType, BlockId blockId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(BlockHeadersDecoratorRepository),
                nameof(GetAsync));
            var operationId = $"{blockchainType}-{blockId}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _blockHeadersRepository.GetAsync(blockchainType, blockId));
        }

        public async Task TryRemoveAsync(string blockchainType, BlockId blockId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(BlockHeadersDecoratorRepository),
                nameof(TryRemoveAsync));
            var operationId = $"{blockchainType}-{blockId}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _blockHeadersRepository.TryRemoveAsync(blockchainType, blockId));
        }
    }
}
