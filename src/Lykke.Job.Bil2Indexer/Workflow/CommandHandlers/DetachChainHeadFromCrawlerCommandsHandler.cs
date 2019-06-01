using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Infrastructure;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.CommandHandlers
{
    public class DetachChainHeadFromCrawlerCommandsHandler : IMessageHandler<DetachChainHeadFromCrawlerCommand>
    {
        private readonly ILog _log;
        private readonly IChainHeadsRepository _chainHeadsRepository;
        
        public DetachChainHeadFromCrawlerCommandsHandler(
            ILogFactory logFactory,
            IChainHeadsRepository chainHeadsRepository)
        {
            _log = logFactory.CreateLog(this);
            _chainHeadsRepository = chainHeadsRepository;
        }

        public async Task<MessageHandlingResult> HandleAsync(DetachChainHeadFromCrawlerCommand command, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = ChainHeadCorrelationId.Parse(headers.CorrelationId);
            var chainHead = await _chainHeadsRepository.GetAsync(command.BlockchainType);
            var chainHeadCorrelationId = chainHead.GetCorrelationId();

            if (messageCorrelationId.IsLegacyRelativeTo(chainHeadCorrelationId) &&
                // In case of retry after chain head sequence incremented and saved,
                // the message is became previous relative to the updated chain head,
                // we should process the message, since we not sure if the events
                // are published.
                !messageCorrelationId.IsPreviousOf(chainHeadCorrelationId))
            {
                // The message is legacy, it already was processed for sure, we can ignore it.
                _log.LogLegacyMessage(command, headers);

                return MessageHandlingResult.Success();
            }

            if (messageCorrelationId.IsPrematureRelativeTo(chainHeadCorrelationId))
            {
                // The message is premature, it can't be processed yet, we should retry it later.
                return MessageHandlingResult.TransientFailure();
            }

            if (chainHead.CanDetachFromCrawler(messageCorrelationId.ModeSequence))
            {
                chainHead.DetachFromCrawler();

                await _chainHeadsRepository.SaveAsync(chainHead);

                chainHeadCorrelationId = chainHead.GetCorrelationId();
            }

            if (messageCorrelationId.IsPreviousOf(chainHeadCorrelationId))
            {
                if (chainHead.IsCatchCrawlerUp)
                {
                    _log.Info("Chain head detached from the crawler", new
                    {
                        Headers = headers, 
                        Message = command, 
                        ChainHead = chainHead
                    });

                    replyPublisher.Publish
                    (
                        new ChainHeadDetachedFromCrawlerEvent
                        {
                            BlockchainType = command.BlockchainType, 
                            BlockNumber = command.BlockNumber
                        },
                        chainHeadCorrelationId.ToString()
                    );
                }
            }

            return MessageHandlingResult.Success();
        }
    }
}
