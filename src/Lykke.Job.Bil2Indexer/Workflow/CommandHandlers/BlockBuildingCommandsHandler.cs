using System;
using System.Diagnostics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class BlockBuildingCommandsHandler :
        IMessageHandler<RollbackBlockCommand>,
        IMessageHandler<WaitForBlockAssemblingCommand>
    {
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly ICrawlersManager _crawlersManager;
        private readonly ITransactionsRepository _transactionsRepository;

        public BlockBuildingCommandsHandler(
            IBlockHeadersRepository blockHeadersRepository,
            ICrawlersManager crawlersManager,
            ITransactionsRepository transactionsRepository)
        {
            _blockHeadersRepository = blockHeadersRepository;
            _crawlersManager = crawlersManager;
            _transactionsRepository = transactionsRepository;
        }

        public async Task HandleAsync(WaitForBlockAssemblingCommand command, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var crawler = await _crawlersManager.GetCrawlerAsync(command.BlockchainType, messageCorrelationId.Configuration);

            if (!crawler.GetCorrelationId().Equals(messageCorrelationId))
            {
                // Disordered message, we should ignore it.
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var timeout = TimeSpan.FromMilliseconds(500);

            do
            {
                var block = await _blockHeadersRepository.GetAsync(command.BlockchainType, command.BlockId);
                var storedTransactionsCount =
                    await _transactionsRepository.CountInBlockAsync(command.BlockchainType, command.BlockId);

                if (block.TransactionsCount == storedTransactionsCount)
                {
                    // TODO: Update block state
                    // await _blockFlagsRepository.SetAsync(command.BlockchainType, command.BlockId, BlockFlags.Assembled);

                    replyPublisher.Publish(new BlockAssembledEvent
                    {
                        BlockchainType = command.BlockchainType,
                        BlockId = command.BlockId
                    });

                    return;
                }

                // TODO: Use retry with specific timeout when it will be implemented

                await Task.Delay(TimeSpan.FromMilliseconds(50));

            } while (stopwatch.Elapsed < timeout);

            // TODO: Silent retry.

            throw new InvalidOperationException("Block assembling awaiting timeout. Command will be retried");
        }

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
