using System;
using System.Threading.Tasks;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.CommandHandlers
{
    public class ReduceChainHeadCommandsHandler : IMessageHandler<ReduceChainHeadCommand>
    {
        private readonly IChainHeadsRepository _chainHeadsRepository;
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly IntegrationSettingsProvider _settingsProvider;

        public ReduceChainHeadCommandsHandler(
            IChainHeadsRepository chainHeadsRepository,
            IBlockHeadersRepository blockHeadersRepository,
            IntegrationSettingsProvider settingsProvider)
        {
            _chainHeadsRepository = chainHeadsRepository;
            _blockHeadersRepository = blockHeadersRepository;
            _settingsProvider = settingsProvider;
        }

        public async Task<MessageHandlingResult> HandleAsync(ReduceChainHeadCommand command, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var chainHead = await _chainHeadsRepository.GetAsync(command.BlockchainType);
            
            if (!(chainHead.CanReduceTo(command.ToBlockNumber) ||
                  chainHead.IsOnBlock(command.ToBlockNumber)))
            {
                // TODO: Not sure yet what to do here. Probably we need to check block header state.
                // We need to determine somehow if this message is outdated or premature and ignore or 
                // retry it correspondingly.
                return MessageHandlingResult.Success();
            }

            var previousBlockNumber = command.ToBlockNumber - 1;
            var previousBlock = await _blockHeadersRepository.GetOrDefaultAsync(command.BlockchainType, previousBlockNumber);

            var settings = _settingsProvider.Get(command.BlockchainType);

            if (settings.Capabilities.TransferModel != BlockchainTransferModel.Amount &&
                settings.Capabilities.TransferModel != BlockchainTransferModel.Coins)
            {
                throw new ArgumentOutOfRangeException(nameof(settings.Capabilities.TransferModel), settings.Capabilities.TransferModel, "");
            }

            if (previousBlock == null ||
                settings.Capabilities.TransferModel == BlockchainTransferModel.Amount && !previousBlock.IsAssembled ||
                settings.Capabilities.TransferModel == BlockchainTransferModel.Coins && !previousBlock.IsExecuted)
            {
                return MessageHandlingResult.Success();
            }

            if (chainHead.CanReduceTo(command.ToBlockNumber))
            {
                chainHead.ReduceTo(command.ToBlockNumber, command.ToBlockId);

                await _chainHeadsRepository.SaveAsync(chainHead);
            }

            // TODO: Update balance snapshots

            replyPublisher.Publish(new ChainHeadReducedEvent
            {
                BlockchainType = command.BlockchainType,
                ChainHeadSequence = chainHead.Version,
                ToBlockNumber = command.ToBlockNumber,
                ToBlockId = command.ToBlockId,
                PreviousBlockId = previousBlock.Id,
                BlockIdToRollback = command.BlockIdToRollback
            });

            return MessageHandlingResult.Success();
        }
    }
}
