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
    public class WaitForChainHeadCommandsHandler : IMessageHandler<WaitForChainHeadCommand>
    {
        private readonly ILog _log;
        private readonly ICrawlersManager _crawlersManager;
        private readonly ICrawlersRepository _crawlersRepository;
        private readonly IChainHeadsRepository _chainHeadsRepository;
        
        public WaitForChainHeadCommandsHandler(
            ILogFactory logFactory,
            ICrawlersManager crawlersManager,
            ICrawlersRepository crawlersRepository,
            IChainHeadsRepository chainHeadsRepository)
        {
            _log = logFactory.CreateLog(this);
            _crawlersManager = crawlersManager;
            _crawlersRepository = crawlersRepository;
            _chainHeadsRepository = chainHeadsRepository;
        }

        public async Task<MessageHandlingResult> HandleAsync(WaitForChainHeadCommand command, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var (crawler, chainHead) = await TaskExecution.WhenAll
            (
                _crawlersManager.GetCrawlerAsync(command.BlockchainType, messageCorrelationId.Configuration),
                _chainHeadsRepository.GetAsync(command.BlockchainType)
            );
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

            if (chainHead.IsCatchCrawlerUp)
            {
                if (crawler.IsIndexing)
                {
                    crawler.StartWaitingForChainHead();

                    await _crawlersRepository.SaveAsync(crawler);
                }

                if (crawler.IsWaitingForChainHead)
                {
                    return MessageHandlingResult.TransientFailure(TimeSpan.FromSeconds(1));
                }
            }

            if (chainHead.IsFollowCrawler)
            {
                if (crawler.IsWaitingForChainHead)
                {
                    crawler.StopWaitingForChainHead();

                    await _crawlersRepository.SaveAsync(crawler);
                }

                if (crawler.IsIndexing)
                {
                    replyPublisher.Publish(new CrawlerCaughtByChainHeadEvent
                    {
                        BlockchainType = command.BlockchainType, 
                        Direction = command.Direction,
                        TargetBlockNumber = command.TargetBlockNumber,
                        OutdatedBlockId = command.OutdatedBlockId,
                        OutdatedBlockNumber = command.OutdatedBlockNumber,
                        TriggeredByBlockId = command.TriggeredByBlockId,
                    });
                }
            }

            return MessageHandlingResult.Success();
        }
    }
}
