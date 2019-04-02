using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;

namespace Lykke.Job.Bil2Indexer.DomainServices
{
    public class ChainHeadsManager : IChainHeadsManager
    {
        private readonly IChainHeadsRepository _chainHeadsRepository;
        private readonly IReadOnlyDictionary<string, long> _firstBlockNumbers;

        public ChainHeadsManager(
            IChainHeadsRepository chainHeadsRepository,
            IReadOnlyDictionary<string, long> firstBlockNumbers)
        {
            _chainHeadsRepository = chainHeadsRepository;
            _firstBlockNumbers = firstBlockNumbers;
        }

        public async Task StartAsync()
        {
            foreach (var (blockchainType, firstBlockNumber) in _firstBlockNumbers)
            {
                var chainHead = await _chainHeadsRepository.GetOrDefaultAsync(blockchainType);

                if (chainHead == null)
                {
                    chainHead = ChainHead.CreateNew(blockchainType, firstBlockNumber);

                    await _chainHeadsRepository.SaveAsync(chainHead);
                }
            }
        }
    }
}
