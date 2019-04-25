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
    public class MoveCrawlerCommandsHandler : IMessageHandler<MoveCrawlerCommand>
    {
        private readonly ICrawlersManager _crawlersManager;
        private readonly ICrawlersRepository _crawlersRepository;
        private readonly ILog _log;

        public MoveCrawlerCommandsHandler(
            ILogFactory logFactory,
            ICrawlersManager crawlersManager,
            ICrawlersRepository crawlersRepository)
        {
            _log = logFactory.CreateLog(this);
            _crawlersManager = crawlersManager;
            _crawlersRepository = crawlersRepository;
        }

        public async Task<MessageHandlingResult> HandleAsync(MoveCrawlerCommand command, MessageHeaders headers, IMessagePublisher eventsPublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var crawler = await _crawlersManager.GetCrawlerAsync(command.BlockchainType, messageCorrelationId.Configuration);
            var crawlerCorrelationId = crawler.GetCorrelationId();

            if (messageCorrelationId.IsLegacyRelativeTo(crawlerCorrelationId) &&
                // In case of retry after crawler sequence incremented and saved,
                // the message is became previous relative to the updated crawler,
                // we should process the message, since we not sure if the event
                // is published.
                !messageCorrelationId.IsPreviousOf(crawlerCorrelationId))
            {
                // The message is legacy, it already was processed for sure, we can ignore it.
                _log.LogLegacyMessage(command, headers);

                return MessageHandlingResult.Success();
            }

            if(messageCorrelationId.IsPrematureRelativeTo(crawlerCorrelationId))
            {
                // The message is premature, it can't be processed yet, we should retry it later.
                return MessageHandlingResult.TransientFailure();
            }

            if (messageCorrelationId.IsTheSameAs(crawlerCorrelationId))
            {
                crawler.MoveTo(command.NextBlockNumber);

                await _crawlersRepository.SaveAsync(crawler);

                crawlerCorrelationId = crawler.GetCorrelationId();
            }

            if (messageCorrelationId.IsPreviousOf(crawlerCorrelationId))
            {
                eventsPublisher.Publish
                (
                    new CrawlerMovedEvent
                    {
                        BlockchainType = command.BlockchainType,
                        BlockNumber = command.NextBlockNumber
                    },
                    crawlerCorrelationId.ToString()
                );
            }

            return MessageHandlingResult.Success();
        }
    }
}
