using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Infrastructure;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.CommandHandlers
{
    public class ExecuteTransferCoinsBlockCommandsHandler : IMessageHandler<ExecuteTransferCoinsBlockCommand>
    {
        private readonly IChainHeadsRepository _chainHeadsRepository;
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly ICoinsRepository _coinsRepository;
        private readonly IBalanceActionsRepository _balanceActionsRepository;
        private readonly IFeeEnvelopesRepository _feeEnvelopesRepository;
        private readonly ILog _log;

        public ExecuteTransferCoinsBlockCommandsHandler(
            ILogFactory logFactory,
            IChainHeadsRepository chainHeadsRepository,
            IBlockHeadersRepository blockHeadersRepository,
            ITransactionsRepository transactionsRepository,
            ICoinsRepository coinsRepository,
            IBalanceActionsRepository balanceActionsRepository,
            IFeeEnvelopesRepository feeEnvelopesRepository)
        {
            _log = logFactory.CreateLog(this);
            _chainHeadsRepository = chainHeadsRepository;
            _blockHeadersRepository = blockHeadersRepository;
            _transactionsRepository = transactionsRepository;
            _coinsRepository = coinsRepository;
            _balanceActionsRepository = balanceActionsRepository;
            _feeEnvelopesRepository = feeEnvelopesRepository;
        }

        public async Task<MessageHandlingResult> HandleAsync(ExecuteTransferCoinsBlockCommand command, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            // This message can be processed in both ChainHead and Crawler flows, but
            // only within ChainHead flow it executes consistently with the ChainHead.
            if (CorrelationIdType.Parse(headers.CorrelationId) == ChainHeadCorrelationId.Type)
            {
                var messageCorrelationId = ChainHeadCorrelationId.Parse(headers.CorrelationId);
                var chainHead = await _chainHeadsRepository.GetAsync(command.BlockchainType);
                var chainHeadCorrelationId = chainHead.GetCorrelationId();

                if (messageCorrelationId.IsLegacyRelativeTo(chainHeadCorrelationId))
                {
                    // The message is legacy, it already was processed for sure, we can ignore it.
                    _log.LogLegacyMessage(command, headers);

                    return MessageHandlingResult.Success();
                }

                if (messageCorrelationId.IsPrematureRelativeTo(chainHeadCorrelationId))
                {
                    // The message is premature, it can't be processed yet, we should retry it later.
                    return MessageHandlingResult.TransientFailure();
                }
            }

            var block = await _blockHeadersRepository.GetOrDefaultAsync(command.BlockchainType, command.BlockId);

            if (block == null)
            {
                // The block can be missed only if the message is legacy. We can ignore it.
                _log.LogLegacyMessage(command, headers);

                return MessageHandlingResult.Success();
            }

            if (block.CanBeExecuted)
            {
                await block.ExecuteAsync
                (
                    _transactionsRepository,
                    _coinsRepository,
                    _balanceActionsRepository,
                    _feeEnvelopesRepository
                );

                await _blockHeadersRepository.SaveAsync(block);
            }

            if (!block.IsExecuted && command.HaveToExecuteEntireBlock)
            {
                throw new InvalidOperationException($"Block should be executed. Actual state: {block.State}");
            }

            if (block.IsExecuted)
            {
                replyPublisher.Publish(new BlockExecutedEvent
                {
                    BlockchainType = command.BlockchainType,
                    BlockId = command.BlockId,
                    BlockNumber = block.Number
                });
            }
            else if(!block.IsPartiallyExecuted)
            {
                throw new InvalidOperationException($"Unexpected block state: {block.State}");
            }
            
            return MessageHandlingResult.Success();
        }
    }
}
