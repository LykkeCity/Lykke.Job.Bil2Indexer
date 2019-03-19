using System;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain.Services;

namespace Lykke.Job.Bil2Indexer.DomainServices
{
    public class ContractEventsPublisher : IContractEventsPublisher
    {
        public Task PublishAsync<TEvent>(TEvent evt) where TEvent : class
        {
            return Task.CompletedTask;
        }
    }
}
