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
        private readonly IBalanceActionsRepository _balanceActionsRepository;
        private readonly IAppInsightTelemetryProvider _appInsightTelemetryProvider;

        public BalanceActionsRepositoryAppInsightDecorator(IBalanceActionsRepository balanceActionsRepository,
            IAppInsightTelemetryProvider appInsightTelemetryProvider)
        {
            _balanceActionsRepository = balanceActionsRepository;
            _appInsightTelemetryProvider = appInsightTelemetryProvider;
        }

        public async Task AddIfNotExistsAsync(string blockchainType, IReadOnlyCollection<BalanceAction> actions)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(BalanceActionsRepositoryAppInsightDecorator),
                nameof(AddIfNotExistsAsync));
            var operationId = $"{blockchainType}-{Guid.NewGuid()}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _balanceActionsRepository.AddIfNotExistsAsync(blockchainType, actions));
        }

        public async Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(BalanceActionsRepositoryAppInsightDecorator),
                nameof(TryRemoveAllOfBlockAsync));
            var operationId = $"{blockchainType}-{blockId}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _balanceActionsRepository.TryRemoveAllOfBlockAsync(blockchainType, blockId));
        }

        public async Task<Money> GetBalanceAsync(string blockchainType, Address address, Asset asset, long atBlockNumber)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(BalanceActionsRepositoryAppInsightDecorator),
                nameof(GetBalanceAsync));
            var operationId = $"{blockchainType}-{address}-{asset.Id}-{atBlockNumber}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _balanceActionsRepository.GetBalanceAsync(blockchainType, address, asset, atBlockNumber));
        }

        public async Task<IReadOnlyDictionary<Asset, Money>> GetBalancesAsync(string blockchainType, Address address, long atBlockNumber)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(BalanceActionsRepositoryAppInsightDecorator),
                nameof(GetBalancesAsync));
            var operationId = $"{blockchainType}-{address}-{atBlockNumber}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _balanceActionsRepository.GetBalancesAsync(blockchainType, address, atBlockNumber));
        }

        public async Task<IReadOnlyDictionary<TransactionId, IReadOnlyDictionary<AccountId, Money>>> GetSomeOfBalancesAsync(string blockchainType, ISet<TransactionId> transactionIds)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(BalanceActionsRepositoryAppInsightDecorator),
                nameof(GetSomeOfBalancesAsync));
            var operationId = $"{blockchainType}-{Guid.NewGuid()}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _balanceActionsRepository.GetSomeOfBalancesAsync(blockchainType, transactionIds));
        }
    }
}
