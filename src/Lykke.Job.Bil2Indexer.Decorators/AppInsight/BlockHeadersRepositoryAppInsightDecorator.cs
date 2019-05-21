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

        public Task SaveAsync(BlockHeader block)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{block.Id}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                () => _impl.SaveAsync(block));
        }

        public Task<BlockHeader> GetOrDefaultAsync(string blockchainType, long blockNumber)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{blockNumber}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                () => _impl.GetOrDefaultAsync(blockchainType, blockNumber));
        }

        public Task<BlockHeader> GetOrDefaultAsync(string blockchainType, BlockId blockId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{blockId}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                () => _impl.GetOrDefaultAsync(blockchainType, blockId));
        }

        public Task<BlockHeader> GetAsync(string blockchainType, BlockId blockId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{blockId}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                () => _impl.GetAsync(blockchainType, blockId));
        }

        public Task<BlockHeader> GetAsync(string blockchainType, DateTime dateTime)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{dateTime}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                () => _impl.GetAsync(blockchainType, dateTime));
        }

        public Task<BlockHeader> GetAsync(string blockchainType, long blockNumber)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{blockNumber}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                () => _impl.GetAsync(blockchainType, blockNumber));
        }

        public Task<IReadOnlyCollection<BlockHeader>> GetCollectionAsync(string blockchainType, int limit, bool orderAsc, long? startingAfter = null,
            long? endingBefore = null)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{limit}-{orderAsc}-{startingAfter}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                () => _impl.GetCollectionAsync(blockchainType, limit, orderAsc, startingAfter, endingBefore));
        }

        public Task TryRemoveAsync(string blockchainType, BlockId blockId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{blockId}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                () => _impl.TryRemoveAsync(blockchainType, blockId));
        }
    }
}
