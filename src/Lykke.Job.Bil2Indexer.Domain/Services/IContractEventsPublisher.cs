using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Services
{
    public interface IContractEventsPublisher
    {
        Task PublishAsync<TEvent>(TEvent evt) where  TEvent : class;
    }
}
