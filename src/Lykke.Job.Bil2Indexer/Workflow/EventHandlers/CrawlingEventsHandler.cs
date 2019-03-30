using System.Threading.Tasks;
using Lykke.Bil2.Client.BlocksReader;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Bil2.Contract.BlocksReader.Commands;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Services;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public class CrawlingEventsHandler :
        IMessageHandler<CrawlerMovedEvent>
    {
        private readonly IBlocksReaderApiFactory _blocksReaderApiFactory;
        private readonly ICrawlersManager _crawlersManager;

        public CrawlingEventsHandler(
            IBlocksReaderApiFactory blocksReaderApiFactory,
            ICrawlersManager crawlersManager)
        {
            _blocksReaderApiFactory = blocksReaderApiFactory;
            _crawlersManager = crawlersManager;
        }

        public async Task HandleAsync(CrawlerMovedEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var crawler = await _crawlersManager.GetCrawlerAsync(evt.BlockchainType, messageCorrelationId.Configuration);
            var crawlerCorrelationId = crawler.GetCorrelationId();

            if (crawlerCorrelationId.Equals(messageCorrelationId))
            {
                // Disordered message, we should ignore it.
                return;
            }

            if (crawler.Configuration.CanProcess(crawler.ExpectedBlockNumber))
            {
                var blocksReaderApi = _blocksReaderApiFactory.Create(evt.BlockchainType);

                await blocksReaderApi.SendAsync(new ReadBlockCommand(evt.BlockNumber), crawler.GetCorrelationId().ToString());
            }
        }
    }
}
