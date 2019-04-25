using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;
using Lykke.Job.Bil2Indexer.Infrastructure;
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
        private readonly ILog _log;

        public WaitForBlockAssemblingCommandsHandler(
            ILogFactory logFactory,
            IBlockHeadersRepository blockHeadersRepository,
            ICrawlersManager crawlersManager,
            ITransactionsRepository transactionsRepository,
            TimeSpan assemblingRetryTimeout)
        {
            _log = logFactory.CreateLog(this);
            _blockHeadersRepository = blockHeadersRepository;
            _crawlersManager = crawlersManager;
            _transactionsRepository = transactionsRepository;
            _assemblingRetryTimeout = assemblingRetryTimeout;
        }

        public async Task<MessageHandlingResult> HandleAsync(WaitForBlockAssemblingCommand command, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var crawler = await _crawlersManager.GetCrawlerAsync(command.BlockchainType, messageCorrelationId.Configuration);
            var crawlerCorrelationId = crawler.GetCorrelationId();

            if (messageCorrelationId.IsLegacyRelativeTo(crawlerCorrelationId))
            {
                // The message is legacy, it already was processed for sure, we can ignore it.
                _log.LogLegacyMessage(command, headers);

                return MessageHandlingResult.Success();
            }

            if (messageCorrelationId.IsPrematureRelativeTo(crawlerCorrelationId))
            {
                // The message is premature, it can't be processed yet, we should retry it later.
                return MessageHandlingResult.TransientFailure();
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
