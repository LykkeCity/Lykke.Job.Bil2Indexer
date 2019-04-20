using System;
using System.Threading.Tasks;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.CommandHandlers
{
    public class WaitForBlockAssemblingCommandsHandler : IMessageHandler<WaitForBlockAssemblingCommand>
    {
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly ICrawlersManager _crawlersManager;
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly TimeSpan _assemblingRetryTimeout;

        public WaitForBlockAssemblingCommandsHandler(
            IBlockHeadersRepository blockHeadersRepository,
            ICrawlersManager crawlersManager,
            ITransactionsRepository transactionsRepository,
            TimeSpan assemblingRetryTimeout)
        {
            _blockHeadersRepository = blockHeadersRepository;
            _crawlersManager = crawlersManager;
            _transactionsRepository = transactionsRepository;
            _assemblingRetryTimeout = assemblingRetryTimeout;
        }

        public async Task<MessageHandlingResult> HandleAsync(WaitForBlockAssemblingCommand command, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var crawler = await _crawlersManager.GetCrawlerAsync(command.BlockchainType, messageCorrelationId.Configuration);

            if (!crawler.GetCorrelationId().Equals(messageCorrelationId))
            {
                // Disordered message, we should ignore it.
                return MessageHandlingResult.Success();
            }

            var block = await _blockHeadersRepository.GetAsync(command.BlockchainType, command.BlockId);

            if (!block.IsAlreadyAssembled)
            {
                if (!await block.TryToAssembleAsync(_transactionsRepository))
                {
                    return MessageHandlingResult.TransientFailure(_assemblingRetryTimeout);
                }

                await _blockHeadersRepository.SaveAsync(block);
            }

            replyPublisher.Publish(new BlockAssembledEvent
            {
                BlockchainType = command.BlockchainType,
                BlockId = command.BlockId
            });

            return MessageHandlingResult.Success();
        }
    }
}
