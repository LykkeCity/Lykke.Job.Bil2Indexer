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
        private readonly ICoinsRepository _impl;

        public CoinsRepositoryAppInsightDecorator(ICoinsRepository coinsRepository,
            IAppInsightTelemetryProvider appInsightTelemetryProvider)
        {
            _impl = coinsRepository;
            _appInsightTelemetryProvider = appInsightTelemetryProvider;
        }

        public Task AddIfNotExistsAsync(IReadOnlyCollection<Coin> coins)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{Guid.NewGuid()}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _impl.AddIfNotExistsAsync(coins));
        }

        public Task SpendAsync(string blockchainType, IReadOnlyCollection<CoinId> ids)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{Guid.NewGuid()}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _impl.SpendAsync(blockchainType, ids));
        }

        public Task RevertSpendingAsync(string blockchainType, IReadOnlyCollection<CoinId> ids)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{Guid.NewGuid()}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _impl.RevertSpendingAsync(blockchainType, ids));
        }

        public Task<IReadOnlyCollection<Coin>> GetSomeOfAsync(string blockchainType, IReadOnlyCollection<CoinId> ids)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{Guid.NewGuid()}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _impl.GetSomeOfAsync(blockchainType, ids));
        }

        public Task RemoveIfExistAsync(string blockchainType, ISet<TransactionId> receivedInTransactionIds)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{Guid.NewGuid()}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _impl.RemoveIfExistAsync(blockchainType, receivedInTransactionIds));
        }
    }
}
