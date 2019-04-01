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
    public class MoveCrawlerCommandsHandler : IMessageHandler<MoveCrawlerCommand>
    {
        private readonly ICrawlersManager _crawlersManager;
        private readonly ICrawlersRepository _crawlersRepository;

        public MoveCrawlerCommandsHandler(
            ICrawlersManager crawlersManager,
            ICrawlersRepository crawlersRepository,
            IBlockHeadersRepository blockHeadersRepository)
        {
            _crawlersManager = crawlersManager;
            _crawlersRepository = crawlersRepository;
        }

        public async Task HandleAsync(MoveCrawlerCommand command, MessageHeaders headers, IMessagePublisher eventsPublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var crawler = await _crawlersManager.GetCrawlerAsync(command.BlockchainType, messageCorrelationId.Configuration);
            var crawlerCorrelationId = crawler.GetCorrelationId();

            if (!(messageCorrelationId.IsPreviousOf(crawlerCorrelationId) || crawlerCorrelationId.Equals(messageCorrelationId)))
            {
                // Disordered message, we should ignore it.
                return;
            }

            crawler.MoveTo(command.NextBlockNumber);

            await _crawlersRepository.SaveAsync(crawler);
            
            eventsPublisher.Publish(new CrawlerMovedEvent
            {
                BlockchainType = command.BlockchainType,
                BlockNumber = command.NextBlockNumber
            });
        }
    }
}
