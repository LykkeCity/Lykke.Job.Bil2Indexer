using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Services
{
    public interface IChainHeadsManager
    {
        Task StartAsync();
    }
}