using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.InMemoryRepositories
{
    public class InMemoryChainHeadsRepository : IChainHeadsRepository
    {
        private readonly ILog _log;
        private readonly ConcurrentDictionary<string, ChainHead> _storage;
        

        public InMemoryChainHeadsRepository(ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);

            _storage = new ConcurrentDictionary<string, ChainHead>();
        }

        public Task<ChainHead> GetOrDefaultAsync(string blockchainType)
        {
            _storage.TryGetValue(blockchainType, out var head);

            return Task.FromResult(head);
        }

        public async Task<ChainHead> GetAsync(string blockchainType)
        {
            var head = await GetOrDefaultAsync(blockchainType);

            if(head == null)
            {
                throw new InvalidOperationException("Chain head not found");
            }

            return head;
        }

        public Task SaveAsync(ChainHead head)
        {
            _storage.AddOrUpdate(
                head.BlockchainType,
                k =>
                {
                    _log.Info($"Chain head saved {head}");

                    return head;
                },
                (k, oldHead) =>
                {
                    if (oldHead.Version != head.Version)
                    {
                        throw new InvalidOperationException($"Optimistic concurrency: chain head version mismatch. Expected version {oldHead.Version}, actual {head.Version}");
                    }

                    var newHead = new ChainHead
                    (
                        head.BlockchainType,
                        head.FirstBlockNumber,
                        head.Version + 1,
                        head.BlockNumber,
                        head.BlockId,
                        head.PreviousBlockId
                    );

                    _log.Info($"Chain head saved {newHead}");

                    return newHead;
                });
            
            return Task.CompletedTask;
        }
    }
}
