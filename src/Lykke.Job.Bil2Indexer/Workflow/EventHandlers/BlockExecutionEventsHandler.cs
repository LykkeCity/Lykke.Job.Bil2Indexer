using System.Threading.Tasks;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public class BlockExecutionEventsHandler : 
        IMessageHandler<BlockExecutedEvent>,
        IMessageHandler<BlockPartiallyExecutedEvent>
    {
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly IChainHeadsRepository _chainHeadsRepository;

        public BlockExecutionEventsHandler(
            IBlockHeadersRepository blockHeadersRepository,
            IChainHeadsRepository chainHeadsRepository)
        {
            _blockHeadersRepository = blockHeadersRepository;
            _chainHeadsRepository = chainHeadsRepository;
        }

        public async Task HandleAsync(BlockExecutedEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            await ContinueExecutionChainAsync(evt.BlockchainType, evt.BlockNumber, headers, replyPublisher);

            var chainHead = await _chainHeadsRepository.GetAsync(evt.BlockchainType);
            
            // TODO: Need to check if message is disordered and ignore/retry it

            if (chainHead.CanExtendTo(evt.BlockNumber))
            {
                replyPublisher.Publish(new ExtendChainHeadCommand
                {
                    BlockchainType = evt.BlockchainType,
                    NextBlockNumber = evt.BlockNumber,
                    NextBlockId = evt.BlockId,
                    ChainHeadVersion = chainHead.Version
                });
            }
        }
        
        public async Task HandleAsync(BlockPartiallyExecutedEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            await ContinueExecutionChainAsync(evt.BlockchainType, evt.BlockNumber, headers, replyPublisher);
        }

        private async Task ContinueExecutionChainAsync(
            string blockchainType, 
            long blockNumber, 
            MessageHeaders headers, 
            IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var nextBlockNumber = blockNumber + 1;

            if (messageCorrelationId.Configuration.CanProcess(nextBlockNumber))
            {
                // If next block is within the crawler assembling range, then the block will be executed
                // after assembling aborting execution chain.
                return;
            }

            // If the next block is out of the crawler assembling range, the crawler should
            // execute it, since it probably is not executed yet because of missed coins.

            var block = await _blockHeadersRepository.GetOrDefaultAsync(blockchainType, nextBlockNumber);

            if (block == null || block.IsNotExecutedYet)
            {
                // If block is not executed yet, then execution chain should be aborted,
                // since block owning crawler will execute it them self and start new 
                // execution chain.

                return;
            }

            if (block.IsPartiallyExecuted)
            {
                // If block is partially executed by previous crawlers, then given crawler
                // should try it again, since probably required coins is exist now.

                replyPublisher.Publish(new ExecuteTransferCoinsBlockCommand
                {
                    BlockchainType = blockchainType,
                    BlockId = block.Id,
                    BlockVersion = block.Version
                });
            }

            // TODO: Some kind of RLE algorithm can be applied here to optimize searching of the next block to execute

            if (block.IsExecuted)
            {
                // If block is executed by previous crawlers, then given crawler
                // should pass it through the execution again, just to continue execution chain.
                // Chain should be continued, since next blocks can be partially executed and
                // should be executed again.

                replyPublisher.Publish(new ExecuteTransferCoinsBlockCommand
                {
                    BlockchainType = blockchainType,
                    BlockId = block.Id,
                    BlockVersion = block.Version
                });
            }
        }
    }
}
