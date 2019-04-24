using System;
using System.Threading.Tasks;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Contract.Events;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations;
using Lykke.Job.Bil2Indexer.Workflow.Commands;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public class ChainHeadExtendedEventsHandler : IMessageHandler<ChainHeadExtendedEvent>
    {
        private readonly IChainHeadsRepository _chainHeadsRepository;
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly IntegrationSettingsProvider _settingsProvider;

        public ChainHeadExtendedEventsHandler(
            IChainHeadsRepository chainHeadsRepository,
            IBlockHeadersRepository blockHeadersRepository,
            IntegrationSettingsProvider settingsProvider)
        {
            _chainHeadsRepository = chainHeadsRepository;
            _blockHeadersRepository = blockHeadersRepository;
            _settingsProvider = settingsProvider;
        }

        public async Task<MessageHandlingResult> HandleAsync(ChainHeadExtendedEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = ChainHeadCorrelationId.Parse(headers.CorrelationId);
            var chainHead = await _chainHeadsRepository.GetAsync(evt.BlockchainType);
            var chainHeadCorrelationId = chainHead.GetCorrelationId();

            if (messageCorrelationId.IsLegacyRelativeTo(chainHeadCorrelationId))
            {
                // The message is legacy, it already was processed for sure, we can ignore it.
                return MessageHandlingResult.Success();
            }

            if (messageCorrelationId.IsPrematureRelativeTo(chainHeadCorrelationId))
            {
                // The message is premature, it can't be processed yet, we should retry it later.
                return MessageHandlingResult.TransientFailure();
            }

            var settings = _settingsProvider.Get(evt.BlockchainType);
            var nextBlockNumber = evt.BlockNumber + 1;
            var nextBlock = await _blockHeadersRepository.GetOrDefaultAsync(evt.BlockchainType, nextBlockNumber);

            if (nextBlock == null)
            {
                // If the next block header is not even received yet, the chain head will be extended once block
                // is assembled. This will be triggered by BlockAssembledEvent.
                return MessageHandlingResult.Success();
            }

            if (settings.Capabilities.TransferModel == BlockchainTransferModel.Amount)
            {
                if (nextBlock.IsAssembled)
                {
                    // If the next block is assembled already, we should extend the chain head bypassing BlockAssembledEvent.
                    replyPublisher.Publish(new ExtendChainHeadCommand
                    {
                        BlockchainType = evt.BlockchainType,
                        ToBlockNumber = nextBlock.Number,
                        ToBlockId = nextBlock.Id
                    });
                }
            }
            else if(settings.Capabilities.TransferModel == BlockchainTransferModel.Coins)
            {
                if (nextBlock.IsPartiallyExecuted)
                {
                    // If the next block is partially executed, we should execute it again
                    // since all data for the execution should be in-place now. We need to do it bypassing BlockAssembledEvent.
                    replyPublisher.Publish(new ExecuteTransferCoinsBlockCommand
                    {
                        BlockchainType = evt.BlockchainType,
                        BlockId = nextBlock.Id
                    });
                } 
                else if (nextBlock.IsExecuted)
                {
                    // If the next block is executed already, we can just extend the chain head to it.
                    replyPublisher.Publish(new ExtendChainHeadCommand
                    {
                        BlockchainType = evt.BlockchainType,
                        ToBlockNumber = nextBlock.Number,
                        ToBlockId = nextBlock.Id
                    });
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(settings.Capabilities.TransferModel), settings.Capabilities.TransferModel, "Unknown transfer model");
            }

            return MessageHandlingResult.Success();
        }
    }
}
