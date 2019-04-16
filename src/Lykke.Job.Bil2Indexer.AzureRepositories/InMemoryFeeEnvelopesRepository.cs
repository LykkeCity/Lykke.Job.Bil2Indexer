using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.AzureRepositories
{
    public class InMemoryFeeEnvelopesRepository : IFeeEnvelopesRepository
    {
        private readonly ConcurrentDictionary<(string, string), ConcurrentDictionary<Asset, FeeEnvelope>> _transactionsFees;
        private readonly ConcurrentDictionary<(string, string), ConcurrentDictionary<Asset, FeeEnvelope>> _blocksFees;

        public InMemoryFeeEnvelopesRepository()
        {
            _transactionsFees = new ConcurrentDictionary<(string, string), ConcurrentDictionary<Asset, FeeEnvelope>>();
            _blocksFees = new ConcurrentDictionary<(string, string), ConcurrentDictionary<Asset, FeeEnvelope>>();
        }

        public Task SaveAsync(IReadOnlyCollection<FeeEnvelope> fees)
        {
            foreach (var fee in fees)
            {
                _transactionsFees.AddOrUpdate
                (
                    (fee.BlockchainType, fee.TransactionId),
                    key =>
                    {
                        var transactionFees = new ConcurrentDictionary<Asset, FeeEnvelope>();

                        transactionFees.TryAdd(fee.Fee.Asset, fee);

                        return transactionFees;
                    },
                    (key, transactionFees) =>
                    {
                        transactionFees.TryAdd(fee.Fee.Asset, fee);

                        return transactionFees;
                    });

                _blocksFees.AddOrUpdate
                (
                    (fee.BlockchainType, fee.BlockId),
                    key =>
                    {
                        var blockFees = new ConcurrentDictionary<Asset, FeeEnvelope>();

                        blockFees.TryAdd(fee.Fee.Asset, fee);

                        return blockFees;
                    },
                    (key, blockFees) =>
                    {
                        blockFees.TryAdd(fee.Fee.Asset, fee);

                        return blockFees;
                    });
            }

            return Task.CompletedTask;
        }

        public Task<FeeEnvelope> GetOrDefaultAsync(string blockchainType, string transactionId, Asset asset)
        {
            if (_transactionsFees.TryGetValue((blockchainType, transactionId), out var transactionFees))
            {
                transactionFees.TryGetValue(asset, out var fee);

                return Task.FromResult(fee);
            }

            return Task.FromResult(default(FeeEnvelope));
        }

        public Task<FeeEnvelope> GetAsync(string blockchainType, string transactionId, Asset asset)
        {
            var fee = GetOrDefaultAsync(blockchainType, transactionId, asset);

            if (fee == null)
            {
                throw new InvalidOperationException($"Fee {blockchainType}:{transactionId}:{asset} not found");
            }

            return fee;
        }

        public Task<PaginatedItems<FeeEnvelope>> GetTransactionFeesAsync(string blockchainType, string transactionId, long limit, string continuation)
        {
            if (_transactionsFees.TryGetValue((blockchainType, transactionId), out var transactionFees))
            {
                return Task.FromResult(PaginatedItems.From(null, transactionFees.Values.ToArray()));
            }

            return Task.FromResult(PaginatedItems<FeeEnvelope>.Empty);
        }

        public Task<PaginatedItems<FeeEnvelope>> GetBlockFeesAsync(string blockchainType, string blockId, long limit, string continuation)
        {
            if (_blocksFees.TryGetValue((blockchainType, blockId), out var blockFees))
            {
                return Task.FromResult(PaginatedItems.From(null, blockFees.Values.ToArray()));
            }

            return Task.FromResult(PaginatedItems<FeeEnvelope>.Empty);
        }

        public Task TryRemoveAllOfBlockAsync(string blockchainType, string blockId)
        {
            if (_blocksFees.TryGetValue((blockchainType, blockId), out var blockFees))
            {
                foreach (var feeEnvelope in blockFees.Values)
                {
                    _transactionsFees.TryRemove((blockchainType, feeEnvelope.TransactionId), out _);
                }
            }

            _blocksFees.TryRemove((blockchainType, blockId), out _);

            return Task.CompletedTask;
        }
    }
}
