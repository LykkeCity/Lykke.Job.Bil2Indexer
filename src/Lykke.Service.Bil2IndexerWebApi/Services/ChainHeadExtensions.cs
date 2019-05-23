using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    internal static class ChainHeadExtensions
    {
        public static async Task<long> GetChainHeadNumberAsync(this IChainHeadsRepository chainHeadsRepository, 
            string blockchainType)
        {
            return (await chainHeadsRepository.GetAsync(blockchainType)).BlockNumber ?? 0;
        }
    }
}
