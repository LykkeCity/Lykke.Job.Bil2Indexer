using System;
using System.Threading.Tasks;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public class ChainHeadExtendedEventsHandler : IMessageHandler<ChainHeadExtendedEvent>
    {
        private readonly IChainHeadsRepository _chainHeadsRepository;
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly IntegrationSettingsProvider _settingsProvider;

        public ChainHeadExtendedEventsHandler(
            IChainHeadsRepository chainHeadsRepository,
            IBlockHeadersRepository blockHeadersRepository,
            IntegrationSettingsProvider settingsProvider)
        {
            _chainHeadsRepository = chainHeadsRepository;
            _blockHeadersRepository = blockHeadersRepository;
            _settingsProvider = settingsProvider;
        }

        public async Task HandleAsync(ChainHeadExtendedEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var settings = _settingsProvider.Get(evt.BlockchainType);

            if (settings.Capabilities.TransferModel == BlockchainTransferModel.Amount)
            {
                var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
                var nextBlockNumber = evt.ToBlockNumber + 1;

                if (messageCorrelationId.Configuration.CanProcess(nextBlockNumber))
                {
                    // If the next block is within the crawler assembling range, then the chain head will
                    // be extended after ending of the assembling.
                    return;
                }

                var chainHead = await _chainHeadsRepository.GetAsync(evt.BlockchainType);

                if (!chainHead.CanExtendTo(nextBlockNumber))
                {
                    return;
                }

                var nextBlock = await _blockHeadersRepository.GetOrDefaultAsync(evt.BlockchainType, nextBlockNumber);

                if (nextBlock == null || !nextBlock.IsAssembled)
                {
                    // If the next block block is not assembled yet no bypassing of the BlockAssembledEvent is
                    // required.
                    return;
                }

                replyPublisher.Publish(new ExtendChainHeadCommand
                {
                    BlockchainType = evt.BlockchainType,
                    ToBlockNumber = nextBlock.Number,
                    ToBlockId = nextBlock.Id,
                    ChainHeadVersion = chainHead.Version
                });
            }
            else if(settings.Capabilities.TransferModel != BlockchainTransferModel.Coins)
            {
                throw new ArgumentOutOfRangeException(nameof(settings.Capabilities.TransferModel), settings.Capabilities.TransferModel, "Unknown transfer model");
            }
        }
    }
}
