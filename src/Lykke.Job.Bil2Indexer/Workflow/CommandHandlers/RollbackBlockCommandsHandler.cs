using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Workflow.Commands;

namespace Lykke.Job.Bil2Indexer.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class RollbackBlockCommandsHandler : IMessageHandler<RollbackBlockCommand>
    {
        public async Task HandleAsync(RollbackBlockCommand command, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            //await Task.WhenAll
            //(
            //    _blockHeadersRepository.RemoveAsync(command.BlockchainType, command.BlockId),
            //    _transactionsRepository.RemoveAllOfBlockAsync(command.BlockchainType, command.BlockId),
            //    _blockFlagsRepository.SetAsync(command.BlockchainType, command.BlockId, BlockFlags.RolledBack),
            //    _coinsRepository.RemoveAllOfBlockAsync(command.BlockchainType, command.BlockId),
            //    _balanceActionsRepository.RemoveAllOfBlockActionsAsync(command.BlockchainType, command.BlockId)
            //);

            //// TODO: Move latestCompletedBlockNumber to the previous block, if it's greater the block being rolled back.
            //// TODO: Rollback transaction actions
            //// TODO: Restore unspent coins

            //replyPublisher.Publish(new BlockRolledBackEvent
            //{
            //    BlockchainType = command.BlockchainType,
            //    BlockNumber = command.BlockNumber,
            //    BlockId = command.BlockId,
            //    PreviousBlockId = command.PreviousBlockId
            //});
        }
    }
}