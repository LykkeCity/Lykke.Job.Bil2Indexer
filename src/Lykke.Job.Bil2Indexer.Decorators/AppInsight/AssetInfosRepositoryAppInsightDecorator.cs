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
        private readonly IAssetInfosRepository _assetInfosRepository;
        private readonly IAppInsightTelemetryProvider _appInsightTelemetryProvider;

        public AssetInfosRepositoryAppInsightDecorator(IAssetInfosRepository assetInfosRepository,
            IAppInsightTelemetryProvider appInsightTelemetryProvider)
        {
            _assetInfosRepository = assetInfosRepository;
            _appInsightTelemetryProvider = appInsightTelemetryProvider;
        }

        public async Task AddIfNotExistsAsync(IEnumerable<AssetInfo> assetInfos)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_assetInfosRepository);
            var operationId = Guid.NewGuid().ToString();

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName, 
                operationId, 
                async () => await _assetInfosRepository.AddIfNotExistsAsync(assetInfos));
        }

        public async Task<AssetInfo> GetOrDefaultAsync(string blockchainType, Asset asset)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_assetInfosRepository);
            var operationId = $"{blockchainType}-{asset?.Id}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _assetInfosRepository.GetOrDefaultAsync(blockchainType, asset));
        }

        public async Task<AssetInfo> GetAsync(string blockchainType, Asset asset)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_assetInfosRepository);
            var operationId = $"{blockchainType}-{asset?.Id}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _assetInfosRepository.GetAsync(blockchainType, asset));
        }

        public async Task<IReadOnlyCollection<AssetInfo>> GetSomeOfAsync(string blockchainType, IEnumerable<Asset> assets)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_assetInfosRepository);
            var operationId = $"{blockchainType}-{Guid.NewGuid()}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _assetInfosRepository.GetSomeOfAsync(blockchainType, assets));
        }

        public async Task<PaginatedItems<AssetInfo>> GetAllAsync(string blockchainType, int limit, string continuation)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_assetInfosRepository);
            var operationId = $"{blockchainType}-{limit}-{continuation}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _assetInfosRepository.GetAllAsync(blockchainType, limit, continuation));
        }
    }
}
