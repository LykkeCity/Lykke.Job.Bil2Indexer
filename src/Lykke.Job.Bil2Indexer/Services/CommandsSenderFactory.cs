using Lykke.Bil2.RabbitMq;
using Lykke.Bil2.RabbitMq.Publication;

namespace Lykke.Job.Bil2Indexer.Services
{
    public class CommandsSenderFactory : ICommandsSenderFactory
    {
        private readonly IRabbitMqEndpoint _rabbitMqEndpoint;

        public CommandsSenderFactory(IRabbitMqEndpoint rabbitMqEndpoint)
        {
            _rabbitMqEndpoint = rabbitMqEndpoint;
        }

        public IMessagePublisher Create()
        {
            return _rabbitMqEndpoint.CreatePublisher(RabbitMqConfigurator.CommandsExchangeName);
        }
    }
}
