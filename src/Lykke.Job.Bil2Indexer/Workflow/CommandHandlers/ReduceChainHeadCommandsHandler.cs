using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Bil2.SharedDomain;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Contract.Events;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Infrastructure;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations;
using Lykke.Job.Bil2Indexer.Workflow.Commands;

namespace Lykke.Job.Bil2Indexer.Workflow.CommandHandlers
{
    public class ReduceChainHeadCommandsHandler : IMessageHandler<ReduceChainHeadCommand>
    {
        private readonly ILog _log;
        private readonly IChainHeadsRepository _chainHeadsRepository;
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly IntegrationSettingsProvider _settingsProvider;
        private readonly IBalanceActionsRepository _balanceActionsRepository;
        private readonly IFeeEnvelopesRepository _feeEnvelopesRepository;
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly ICoinsRepository _coinsRepository;
        
        public ReduceChainHeadCommandsHandler(
            ILogFactory logFactory,
            IChainHeadsRepository chainHeadsRepository,
            IBlockHeadersRepository blockHeadersRepository,
            IntegrationSettingsProvider settingsProvider,
            IBalanceActionsRepository balanceActionsRepository,
            IFeeEnvelopesRepository feeEnvelopesRepository,
            ITransactionsRepository transactionsRepository,
            ICoinsRepository coinsRepository)
        {
            _log = logFactory.CreateLog(this);
            _chainHeadsRepository = chainHeadsRepository;
            _blockHeadersRepository = blockHeadersRepository;
            _settingsProvider = settingsProvider;
            _balanceActionsRepository = balanceActionsRepository;
            _feeEnvelopesRepository = feeEnvelopesRepository;
            _transactionsRepository = transactionsRepository;
            _coinsRepository = coinsRepository;
        }

        public async Task<MessageHandlingResult> HandleAsync(ReduceChainHeadCommand command, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = ChainHeadCorrelationId.Parse(headers.CorrelationId);
            var (chainHead, block) = await TaskExecution.WhenAll
            (
                _chainHeadsRepository.GetAsync(command.BlockchainType),
                _blockHeadersRepository.GetAsync(command.BlockchainType, command.ToBlockNumber)
            );
            var chainHeadCorrelationId = chainHead.GetCorrelationId();

            if (messageCorrelationId.IsLegacyRelativeTo(chainHeadCorrelationId) &&
                // In case of retry after chain head sequence incremented and saved,
                // the message is became previous relative to the updated chain head,
                // we should process the message, since we not sure if the events
                // are published.
                !messageCorrelationId.IsPreviousOf(chainHeadCorrelationId))
            {
                // The message is legacy, it already was processed for sure, we can ignore it.
                _log.LogLegacyMessage(command, headers);

                return MessageHandlingResult.Success();
            }

            if(messageCorrelationId.IsPrematureRelativeTo(chainHeadCorrelationId))
            {
                // The message is premature, it can't be processed yet, we should retry it later.
                return MessageHandlingResult.TransientFailure();
            }
            
            if (messageCorrelationId.IsTheSameAs(chainHeadCorrelationId))
            {
                chainHead.ReduceTo(command.ToBlockNumber, block.Id);

                await Task.WhenAll
                (
                    RemoveBlockAsync(command.BlockchainType, command.OutdatedBlockId),
                    // Removes new block. It will be read once again on moving forward.
                    // It could be optimized in the future - we can keep new block on moving backward,
                    // but in this case special logic required to remove such block in case
                    // when one more chain branch is created while given fork is processed.
                    // Such optimization complicates overall workflow and not so valuable since
                    // forks should be not very frequent.
                    RemoveBlockAsync(command.BlockchainType, command.TriggeredByBlockId)
                );

                await _chainHeadsRepository.SaveAsync(chainHead);
            }

            if (messageCorrelationId.IsPreviousOf(chainHeadCorrelationId))
            {
                _log.Info("Chain head reduced", new
                {
                    Headers = headers,
                    Message = command,
                    ChainHead = chainHead
                });

                replyPublisher.Publish(new ChainHeadReducedEvent
                {
                    BlockchainType = command.BlockchainType,
                    ChainHeadSequence = chainHead.Version,
                    ToBlockNumber = command.ToBlockNumber,
                    ToBlockId = block.Id,
                    PreviousBlockId = block.PreviousBlockId,
                    OutdatedBlockId = command.OutdatedBlockId,
                    OutdatedBlockNumber = command.OutdatedBlockNumber,
                    TriggeredByBlockId = command.TriggeredByBlockId
                });
            }

            return MessageHandlingResult.Success();
        }

        private async Task RemoveBlockAsync(string blockchainType, BlockId blockId)
        {
            var removeReceivedCoinsTask = Task.CompletedTask;
            var removeBalanceActionsTask = Task.CompletedTask;
            var removeFeeEnvelopesTask = _feeEnvelopesRepository.TryRemoveAllOfBlockAsync(blockchainType, blockId);
            var settings = _settingsProvider.Get(blockchainType);

            if (settings.Capabilities.TransferModel == BlockchainTransferModel.Coins)
            {
                removeReceivedCoinsTask = _coinsRepository.RemoveIfExistAsync(blockchainType, blockId);

                var blockHeader = await _blockHeadersRepository.GetAsync(blockchainType, blockId);

                if (blockHeader.ExecutionCanBeReverted)
                {
                    await blockHeader.RevertExecutionAsync
                    (
                        _balanceActionsRepository,
                        _transactionsRepository,
                        _coinsRepository
                    );
                }               
            }
            else if (settings.Capabilities.TransferModel == BlockchainTransferModel.Amount)
            {
                removeBalanceActionsTask = _balanceActionsRepository.TryRemoveAllOfBlockAsync(blockchainType, blockId);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(settings.Capabilities.TransferModel), settings.Capabilities.TransferModel, "");
            }

            await Task.WhenAll
            (
                removeFeeEnvelopesTask,
                removeReceivedCoinsTask,
                removeBalanceActionsTask,
                _blockHeadersRepository.TryRemoveAsync(blockchainType, blockId),
                _transactionsRepository.TryRemoveAllOfBlockAsync(blockchainType, blockId)
            );
        }
    }
}
