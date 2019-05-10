using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Contract;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services.Infrastructure;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.Decorators.AppInsight
{
    [UsedImplicitly]
    public class BalanceActionsRepositoryAppInsightDecorator : IBalanceActionsRepository
    {
        private readonly IBalanceActionsRepository _impl;
        private readonly IAppInsightTelemetryProvider _appInsightTelemetryProvider;

        public BalanceActionsRepositoryAppInsightDecorator(IBalanceActionsRepository balanceActionsRepository,
            IAppInsightTelemetryProvider appInsightTelemetryProvider)
        {
            _impl = balanceActionsRepository;
            _appInsightTelemetryProvider = appInsightTelemetryProvider;
        }

        public Task AddIfNotExistsAsync(string blockchainType, IEnumerable<BalanceAction> actions)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{Guid.NewGuid()}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _impl.AddIfNotExistsAsync(blockchainType, actions));
        }

        public Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{blockId}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _impl.TryRemoveAllOfBlockAsync(blockchainType, blockId));
        }

        public Task<Money> GetBalanceAsync(string blockchainType, Address address, Asset asset, long atBlockNumber)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{address}-{asset.Id}-{atBlockNumber}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _impl.GetBalanceAsync(blockchainType, address, asset, atBlockNumber));
        }

        public Task<IReadOnlyDictionary<Asset, Money>> GetBalancesAsync(string blockchainType, Address address, long atBlockNumber)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{address}-{atBlockNumber}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _impl.GetBalancesAsync(blockchainType, address, atBlockNumber));
        }

        public Task<IReadOnlyDictionary<TransactionId, IReadOnlyDictionary<AccountId, Money>>> GetSomeOfBalancesAsync(string blockchainType, ISet<TransactionId> transactionIds)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{Guid.NewGuid()}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _impl.GetSomeOfBalancesAsync(blockchainType, transactionIds));
        }
    }
}
