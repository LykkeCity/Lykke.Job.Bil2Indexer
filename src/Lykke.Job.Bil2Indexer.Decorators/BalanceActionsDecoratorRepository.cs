using System;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Contract;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Decorators
{
    public class BalanceActionsDecoratorRepository : IBalanceActionsRepository
    {
        private IBalanceActionsRepository _balanceActionsRepository;
        private IAppInsightTelemetryProvider _appInsightTelemetryProvider;

        public BalanceActionsDecoratorRepository(IBalanceActionsRepository balanceActionsRepository,
            IAppInsightTelemetryProvider appInsightTelemetryProvider)
        {
            _balanceActionsRepository = balanceActionsRepository;
            _appInsightTelemetryProvider = appInsightTelemetryProvider;
        }

        public async Task AddIfNotExistsAsync(string blockchainType, IReadOnlyCollection<BalanceAction> actions)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(BalanceActionsDecoratorRepository),
                nameof(AddIfNotExistsAsync));
            var operationId = $"{blockchainType}-{Guid.NewGuid()}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _balanceActionsRepository.AddIfNotExistsAsync(blockchainType, actions));
        }

        public async Task TryRemoveAllOfBlockAsync(string blockchainType, BlockId blockId)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(BalanceActionsDecoratorRepository),
                nameof(TryRemoveAllOfBlockAsync));
            var operationId = $"{blockchainType}-{blockId}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _balanceActionsRepository.TryRemoveAllOfBlockAsync(blockchainType, blockId));
        }

        public async Task<Money> GetBalanceAsync(string blockchainType, Address address, Asset asset, long atBlockNumber)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(BalanceActionsDecoratorRepository),
                nameof(GetBalanceAsync));
            var operationId = $"{blockchainType}-{address}-{asset.Id}-{atBlockNumber}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _balanceActionsRepository.GetBalanceAsync(blockchainType, address, asset, atBlockNumber));
        }

        public async Task<IReadOnlyDictionary<Asset, Money>> GetBalancesAsync(string blockchainType, Address address, long atBlockNumber)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(BalanceActionsDecoratorRepository),
                nameof(GetBalancesAsync));
            var operationId = $"{blockchainType}-{address}-{atBlockNumber}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _balanceActionsRepository.GetBalancesAsync(blockchainType, address, atBlockNumber));
        }

        public async Task<IReadOnlyDictionary<TransactionId, IReadOnlyDictionary<AccountId, Money>>> GetSomeOfBalancesAsync(string blockchainType, ISet<TransactionId> transactionIds)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(BalanceActionsDecoratorRepository),
                nameof(GetSomeOfBalancesAsync));
            var operationId = $"{blockchainType}-{Guid.NewGuid()}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _balanceActionsRepository.GetSomeOfBalancesAsync(blockchainType, transactionIds));
        }
    }
}
