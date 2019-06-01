using System;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations;
using Lykke.Job.Bil2Indexer.Workflow.Commands;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public static class ChainHeadExtendingHelper
    {
        public static MessageHandlingResult PerformExtendingFlow(
            IMessagePublisher replyPublisher,
            BlockchainIntegrationSettings settings, 
            BlockHeader nextBlock)
        {
            switch (settings.Capabilities.TransferModel)
            {
                case BlockchainTransferModel.Amount when nextBlock.IsNotAssembledYet:
                    // Chain head need to wait for the next block to be assembled.
                    return MessageHandlingResult.TransientFailure(TimeSpan.FromSeconds(1));

                case BlockchainTransferModel.Amount:
                    if (nextBlock.IsAssembled)
                    {
                        // If the next block is assembled already, we should extend the chain head bypassing BlockAssembledEvent.
                        replyPublisher.Publish(new ExtendChainHeadCommand
                        {
                            BlockchainType = nextBlock.BlockchainType,
                            ToBlockNumber = nextBlock.Number,
                            ToBlockId = nextBlock.Id
                        });
                    }

                    break;

                case BlockchainTransferModel.Coins when nextBlock.IsNotExecutedYet:
                    // Chain head need to wait for the next block to be executed or partially executed.
                    return MessageHandlingResult.TransientFailure();

                case BlockchainTransferModel.Coins when nextBlock.IsPartiallyExecuted:
                    // If the next block is partially executed, we should execute it again
                    // since all data for the execution should be in-place now. We need to do it bypassing BlockAssembledEvent.
                    replyPublisher.Publish(new ExecuteTransferCoinsBlockCommand
                    {
                        BlockchainType = nextBlock.BlockchainType,
                        BlockId = nextBlock.Id,
                        HaveToExecuteEntireBlock = true,
                        HaveToExecuteInOrder = true,
                        HaveToExtendChainHead = true
                    });
                    break;

                case BlockchainTransferModel.Coins:
                    if (nextBlock.IsExecuted)
                    {
                        // If the next block is executed already, we can just extend the chain head to it.
                        replyPublisher.Publish(new ExtendChainHeadCommand
                        {
                            BlockchainType = nextBlock.BlockchainType,
                            ToBlockNumber = nextBlock.Number,
                            ToBlockId = nextBlock.Id
                        });
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(settings.Capabilities.TransferModel), settings.Capabilities.TransferModel, "Unknown transfer model");
            }

            return MessageHandlingResult.Success();
        }
    }
}
