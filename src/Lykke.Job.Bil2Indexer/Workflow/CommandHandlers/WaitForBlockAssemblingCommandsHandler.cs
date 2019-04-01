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
    public class WaitForBlockAssemblingCommandsHandler : IMessageHandler<WaitForBlockAssemblingCommand>
    {
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly ICrawlersManager _crawlersManager;
        private readonly ITransactionsRepository _transactionsRepository;

        public WaitForBlockAssemblingCommandsHandler(
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

                if (!block.IsAlreadyAssembled)
                {
                    if (await block.TryToAssembleAsync(_transactionsRepository))
                    {
                        await _blockHeadersRepository.SaveAsync(block);
                    }
                    else
                    {
                        if (stopwatch.Elapsed > timeout)
                        {
                            // TODO: Silent retry.

                            // TODO: If block is not assembled for the configured timeout, it should be read again.

                            throw new InvalidOperationException("Block assembling awaiting timeout. Command will be retried");
                        }

                        // TODO: Use retry with specific timeout when it will be implemented

                        await Task.Delay(TimeSpan.FromMilliseconds(50));

                        continue;
                    }
                }

                replyPublisher.Publish(new BlockAssembledEvent
                {
                    BlockchainType = command.BlockchainType,
                    BlockId = command.BlockId
                });

                break;

            } while (true);
        }
    }
}
