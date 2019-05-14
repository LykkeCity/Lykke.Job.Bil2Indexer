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
    public class AssetInfosRepositoryAppInsightDecorator : IAssetInfosRepository
    {
        private readonly IAssetInfosRepository _impl;
        private readonly IAppInsightTelemetryProvider _appInsightTelemetryProvider;

        public AssetInfosRepositoryAppInsightDecorator(IAssetInfosRepository assetInfosRepository,
            IAppInsightTelemetryProvider appInsightTelemetryProvider)
        {
            _impl = assetInfosRepository;
            _appInsightTelemetryProvider = appInsightTelemetryProvider;
        }

        public Task AddIfNotExistsAsync(IReadOnlyCollection<AssetInfo> assetInfos)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = Guid.NewGuid().ToString();

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName, 
                operationId, 
                () => _impl.AddIfNotExistsAsync(assetInfos));
        }

        public Task<AssetInfo> GetOrDefaultAsync(string blockchainType, Asset asset)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{asset?.Id}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                () => _impl.GetOrDefaultAsync(blockchainType, asset));
        }

        public Task<AssetInfo> GetAsync(string blockchainType, Asset asset)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{asset?.Id}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                () => _impl.GetAsync(blockchainType, asset));
        }

        public Task<IReadOnlyCollection<AssetInfo>> GetSomeOfAsync(string blockchainType, IEnumerable<Asset> assets)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{Guid.NewGuid()}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                () => _impl.GetSomeOfAsync(blockchainType, assets));
        }

        public Task<PaginatedItems<AssetInfo>> GetAllAsync(string blockchainType, int limit, string continuation)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{limit}-{continuation}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                () => _impl.GetAllAsync(blockchainType, limit, continuation));
        }

        public Task<IReadOnlyCollection<AssetInfo>> GetAllAsync(string blockchainType, int limit, bool orderAsc, string startingAfter, string endingBefore)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = string.Join("-", blockchainType, limit.ToString(), orderAsc, startingAfter, endingBefore);

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                () => _impl.GetAllAsync(blockchainType, limit, orderAsc, startingAfter, endingBefore));
        }
    }
}
