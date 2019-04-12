using System;
using System.Threading.Tasks;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.CommandHandlers
{
    public class ExecuteTransferCoinsBlockCommandsHandler : IMessageHandler<ExecuteTransferCoinsBlockCommand>
    {
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly ICoinsRepository _coinsRepository;
        private readonly IBalanceActionsRepository _balanceActionsRepository;
        private readonly IFeeEnvelopesRepository _feeEnvelopesRepository;

        public ExecuteTransferCoinsBlockCommandsHandler(
            IBlockHeadersRepository blockHeadersRepository,
            ITransactionsRepository transactionsRepository,
            ICoinsRepository coinsRepository,
            IBalanceActionsRepository balanceActionsRepository,
            IFeeEnvelopesRepository feeEnvelopesRepository)
        {
            _blockHeadersRepository = blockHeadersRepository;
            _transactionsRepository = transactionsRepository;
            _coinsRepository = coinsRepository;
            _balanceActionsRepository = balanceActionsRepository;
            _feeEnvelopesRepository = feeEnvelopesRepository;
        }

        public async Task HandleAsync(ExecuteTransferCoinsBlockCommand command, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var block = await _blockHeadersRepository.GetOrDefaultAsync(command.BlockchainType, command.BlockId);

            if (block == null || !(block.CanBeExecuted || block.IsExecuted || block.IsPartiallyExecuted))
            {
                // Block either already rolled back, or not ready to be executed yet and
                // not executed yet and not partially executed yet, command should be skipped.
                return;
            }

            if (block.CanBeExecuted)
            {
                await block.ExecuteAsync(_transactionsRepository, _coinsRepository, _balanceActionsRepository, _feeEnvelopesRepository);

                await _blockHeadersRepository.SaveAsync(block);
            }

            if (block.IsPartiallyExecuted)
            {
                replyPublisher.Publish(new BlockPartiallyExecutedEvent
                {
                    BlockchainType = command.BlockchainType,
                    BlockId = command.BlockId,
                    BlockNumber = block.Number
                });
            }
            else if(block.IsExecuted)
            {
                replyPublisher.Publish(new BlockExecutedEvent
                {
                    BlockchainType = command.BlockchainType,
                    BlockId = command.BlockId,
                    BlockNumber = block.Number
                });
            }
            else
            {
                throw new InvalidOperationException($"Unexpected block state: {block.State}");
            }
        }
    }
}
