using Lykke.Bil2.RabbitMq;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Job.Bil2Indexer.Contract;

namespace Lykke.Job.Bil2Indexer.Services
{
    public class MessageSendersFactory : IMessageSendersFactory
    {
        private readonly IRabbitMqEndpoint _rabbitMqEndpoint;

        public MessageSendersFactory(IRabbitMqEndpoint rabbitMqEndpoint)
        {
            _rabbitMqEndpoint = rabbitMqEndpoint;
        }

        public IMessagePublisher CreateCommandsSender()
        {
            return _rabbitMqEndpoint.CreatePublisher(RabbitMqConfigurator.CommandsExchangeName);
        }

        public IMessagePublisher CreateEventsPublisher()
        {
            return _rabbitMqEndpoint.CreatePublisher(Bil2IndexerContractExchanges.Events);
        }
    }
}
