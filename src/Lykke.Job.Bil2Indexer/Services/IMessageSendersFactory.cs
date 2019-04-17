using Lykke.Bil2.RabbitMq.Publication;

namespace Lykke.Job.Bil2Indexer.Services
{
    public interface IMessageSendersFactory
    {
        IMessagePublisher CreateCommandsSender();
        IMessagePublisher CreateEventsPublisher();
    }
}
