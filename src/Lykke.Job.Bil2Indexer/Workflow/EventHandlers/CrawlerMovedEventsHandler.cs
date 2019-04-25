using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Bil2.Contract.BlocksReader.Commands;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Services;
using Lykke.Job.Bil2Indexer.Infrastructure;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public class CrawlerMovedEventsHandler : IMessageHandler<CrawlerMovedEvent>
    {
        private readonly IBlocksReaderApiFactory _blocksReaderApiFactory;
        private readonly ICrawlersManager _crawlersManager;
        private readonly ILog _log;

        public CrawlerMovedEventsHandler(
            ILogFactory logFactory,
            IBlocksReaderApiFactory blocksReaderApiFactory,
            ICrawlersManager crawlersManager)
        {
            _log = logFactory.CreateLog(this);
            _blocksReaderApiFactory = blocksReaderApiFactory;
            _crawlersManager = crawlersManager;
        }

        public async Task<MessageHandlingResult> HandleAsync(CrawlerMovedEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var crawler = await _crawlersManager.GetCrawlerAsync(evt.BlockchainType, messageCorrelationId.Configuration);
            var crawlerCorrelationId = crawler.GetCorrelationId();

            if (messageCorrelationId.IsLegacyRelativeTo(crawlerCorrelationId))
            {
                // The message is legacy, it already was processed for sure, we can ignore it.
                _log.LogLegacyMessage(evt, headers);

                return MessageHandlingResult.Success();
            }

            if (messageCorrelationId.IsPrematureRelativeTo(crawlerCorrelationId))
            {
                // The message is premature, it can't be processed yet, we should retry it later.
                return MessageHandlingResult.TransientFailure();
            }

            if (crawler.Configuration.CanProcess(crawler.ExpectedBlockNumber))
            {
                var blocksReaderApi = _blocksReaderApiFactory.Create(evt.BlockchainType);

                await blocksReaderApi.SendAsync(new ReadBlockCommand(evt.BlockNumber), crawlerCorrelationId.ToString());
            }

            return MessageHandlingResult.Success();
        }
    }
}
