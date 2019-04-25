﻿using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Infrastructure;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public class BlockExecutedEventsHandler : IMessageHandler<BlockExecutedEvent>
    {
        private readonly IChainHeadsRepository _chainHeadsRepository;
        private readonly ILog _log;

        public BlockExecutedEventsHandler(
            ILogFactory logFactory,
            IChainHeadsRepository chainHeadsRepository)
        {
            _log = logFactory.CreateLog(this);
            _chainHeadsRepository = chainHeadsRepository;
        }

        public async Task<MessageHandlingResult> HandleAsync(BlockExecutedEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var chainHead = await _chainHeadsRepository.GetAsync(evt.BlockchainType);

            // This message can be processed in both ChainHead and Crawler flows, but
            // only within ChainHead flow it executes consistently with the ChainHead.
            if (CorrelationIdType.Parse(headers.CorrelationId) == ChainHeadCorrelationId.Type)
            {
                var messageCorrelationId = ChainHeadCorrelationId.Parse(headers.CorrelationId);
                var chainHeadCorrelationId = chainHead.GetCorrelationId();

                if (messageCorrelationId.IsLegacyRelativeTo(chainHeadCorrelationId))
                {
                    // The message is legacy, it already was processed for sure, we can ignore it.
                    _log.LogLegacyMessage(evt, headers);

                    return MessageHandlingResult.Success();
                }

                if (messageCorrelationId.IsPrematureRelativeTo(chainHeadCorrelationId))
                {
                    // The message is premature, it can't be processed yet, we should retry it later.
                    return MessageHandlingResult.TransientFailure();
                }
            }

            if (chainHead.CanExtendTo(evt.BlockNumber))
            {
                replyPublisher.Publish
                (
                    new ExtendChainHeadCommand
                    {
                        BlockchainType = evt.BlockchainType,
                        ToBlockNumber = evt.BlockNumber,
                        ToBlockId = evt.BlockId
                    },
                    // Continues or starts chain head flow
                    chainHead.GetCorrelationId().ToString()
                );
            }

            return MessageHandlingResult.Success();
        }
    }
}
