using System.Threading.Tasks;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Contract.Events;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations;
using Lykke.Job.Bil2Indexer.Workflow.Commands;

namespace Lykke.Job.Bil2Indexer.Workflow.CommandHandlers
{
    public class RollbackBlockCommandsHandler : IMessageHandler<RollbackBlockCommand>
    {
        private readonly IBalanceActionsRepository _balanceActionsRepository;
        private readonly ICoinsRepository _coinsRepository;
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly IntegrationSettingsProvider _settingsProvider;

        public RollbackBlockCommandsHandler(
            IBalanceActionsRepository balanceActionsRepository,
            ICoinsRepository coinsRepository,
            ITransactionsRepository transactionsRepository,
            IBlockHeadersRepository blockHeadersRepository,
            IntegrationSettingsProvider settingsProvider)
        {
            _balanceActionsRepository = balanceActionsRepository;
            _coinsRepository = coinsRepository;
            _transactionsRepository = transactionsRepository;
            _blockHeadersRepository = blockHeadersRepository;
            _settingsProvider = settingsProvider;
        }

        public async Task HandleAsync(RollbackBlockCommand command, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var removeBlockHeaderAndBalanceActionsTask = Task.WhenAll
            (
                _blockHeadersRepository.RemoveAsync(command.BlockchainType, command.BlockId),
                _balanceActionsRepository.RemoveAllOfBlockActionsAsync(command.BlockchainType, command.BlockId)
            );

            var settings = _settingsProvider.Get(command.BlockchainType);

            if (settings.Capabilities.TransferModel == BlockchainTransferModel.Amount)
            {
                // TODO: Remove tx
            }
            else if (settings.Capabilities.TransferModel == BlockchainTransferModel.Coins)
            {
                // TODO: Un-spend coins spent by tx
                // TODO: Remove coins received by tx
                // TODO: Remove tx
            }
            else
            {

            }

            await removeBlockHeaderAndBalanceActionsTask;

            replyPublisher.Publish(new BlockRolledBackEvent
            {
                BlockchainType = command.BlockchainType,
                BlockNumber = command.BlockNumber,
                BlockId = command.BlockId,
                PreviousBlockId = command.PreviousBlockId
            });
        }
    }
}
