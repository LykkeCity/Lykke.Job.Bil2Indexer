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
    public class CoinsRepositoryAppInsightDecorator : ICoinsRepository
    {
        private readonly IAppInsightTelemetryProvider _appInsightTelemetryProvider;
        private readonly ICoinsRepository _coinsRepository;

        public CoinsRepositoryAppInsightDecorator(ICoinsRepository coinsRepository,
            IAppInsightTelemetryProvider appInsightTelemetryProvider)
        {
            _coinsRepository = coinsRepository;
            _appInsightTelemetryProvider = appInsightTelemetryProvider;
        }

        public async Task AddIfNotExistsAsync(IReadOnlyCollection<Coin> coins)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(ChainHeadsRepositoryAppInsightDecorator),
                nameof(AddIfNotExistsAsync));
            var operationId = $"{Guid.NewGuid()}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _coinsRepository.AddIfNotExistsAsync(coins));
        }

        public async Task SpendAsync(string blockchainType, IReadOnlyCollection<CoinId> ids)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(ChainHeadsRepositoryAppInsightDecorator),
                nameof(SpendAsync));
            var operationId = $"{blockchainType}-{Guid.NewGuid()}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _coinsRepository.SpendAsync(blockchainType, ids));
        }

        public async Task RevertSpendingAsync(string blockchainType, IReadOnlyCollection<CoinId> ids)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(ChainHeadsRepositoryAppInsightDecorator),
                nameof(RevertSpendingAsync));
            var operationId = $"{blockchainType}-{Guid.NewGuid()}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _coinsRepository.RevertSpendingAsync(blockchainType, ids));
        }

        public async Task<IReadOnlyCollection<Coin>> GetSomeOfAsync(string blockchainType, IReadOnlyCollection<CoinId> ids)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(ChainHeadsRepositoryAppInsightDecorator),
                nameof(GetSomeOfAsync));
            var operationId = $"{blockchainType}-{Guid.NewGuid()}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _coinsRepository.GetSomeOfAsync(blockchainType, ids));
        }

        public async Task RemoveIfExistAsync(string blockchainType, ISet<TransactionId> receivedInTransactionIds)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(ChainHeadsRepositoryAppInsightDecorator),
                nameof(RemoveIfExistAsync));
            var operationId = $"{blockchainType}-{Guid.NewGuid()}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _coinsRepository.RemoveIfExistAsync(blockchainType, receivedInTransactionIds));
        }
    }
}
