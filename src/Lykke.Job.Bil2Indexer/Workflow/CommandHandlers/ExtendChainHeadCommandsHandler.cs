using System.Threading.Tasks;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.CommandHandlers
{
    public class ExtendChainHeadCommandsHandler : IMessageHandler<ExtendChainHeadCommand>
    {
        private readonly IChainHeadsRepository _chainHeadsRepository;

        public ExtendChainHeadCommandsHandler(IChainHeadsRepository chainHeadsRepository)
        {
            _chainHeadsRepository = chainHeadsRepository;
        }

        public async Task HandleAsync(ExtendChainHeadCommand command, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var chainHead = await _chainHeadsRepository.GetAsync(command.BlockchainType);

            if (!(chainHead.CanExtendTo(command.NextBlockNumber) ||
                chainHead.IsOnBlock(command.NextBlockNumber)))
            {
                // TODO: Not sure yet what to do here. Probably we need to check block header state.
                // We need to determine somehow if this message is outdated or premature and ignore or 
                // retry it correspondingly.
                return;
            }

            if (chainHead.CanExtendTo(command.NextBlockNumber))
            {
                chainHead.ExtendTo(command.NextBlockNumber, command.NextBlockId);

                await _chainHeadsRepository.SaveAsync(chainHead);
            }

            replyPublisher.Publish(new ChainHeadExtendedEvent
            {
                BlockchainType = command.BlockchainType,
                ChainHeadSequence = chainHead.Version,
                BlockNumber = command.NextBlockNumber,
                BlockId = command.NextBlockId
            });
        }
    }
}
