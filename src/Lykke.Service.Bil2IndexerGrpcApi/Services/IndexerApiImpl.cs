using Bil2.Indexer;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Lykke.Job.Bil2Indexer.Contract.Events;
using StackExchange.Redis;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Lykke.Service.Bil2IndexerGrpcApi.Services
{
    public class IndexerApiImpl : IndexerApi.IndexerApiBase
    {
        // todo: unordered messages filtering
        // todo: concurrency
        // todo: subscription persistency: Redis cluster or persistent storage
        // todo: performance: change set into hashset
        // todo: performance: local cache?
        // todo: worng events filtering: LastIrreversibleBlockUpdated, ChainHeadExtended, BlockRolledBack

        private readonly ConnectionMultiplexer _redis;

        private readonly BufferBlock<TransactionExecuted> _transactionExecutedEventsBuffer = new BufferBlock<TransactionExecuted>();
        private readonly ConcurrentDictionary<string, IServerStreamWriter<TransactionExecuted>> _subscriberExecutecTransactions = new ConcurrentDictionary<string, IServerStreamWriter<TransactionExecuted>>();

        private readonly BufferBlock<TransactionFailed> _transactionFailedEventsBuffer = new BufferBlock<TransactionFailed>();
        private readonly ConcurrentDictionary<string, IServerStreamWriter<TransactionFailed>> _subscriberFailedTranscationsMap = new ConcurrentDictionary<string, IServerStreamWriter<TransactionFailed>>();

        private readonly BufferBlock<LastIrreversibleBlockUpdated> _lastIrreversibleBlockUpdatedEventsBuffer = new BufferBlock<LastIrreversibleBlockUpdated>();
        private readonly ConcurrentDictionary<string, IServerStreamWriter<LastIrreversibleBlockUpdated>> _subscriberLastIrreversibleBlockUpdatedMap = new ConcurrentDictionary<string, IServerStreamWriter<LastIrreversibleBlockUpdated>>();

        private readonly BufferBlock<ChainHeadExtended> _chainHeadExtendedEventsBuffer = new BufferBlock<ChainHeadExtended>();
        private readonly ConcurrentDictionary<string, IServerStreamWriter<ChainHeadExtended>> _subscriberChainHeadExtendedMap = new ConcurrentDictionary<string, IServerStreamWriter<ChainHeadExtended>>();

        private readonly BufferBlock<BlockRolledBack> _blockRolledBackEventsBuffer = new BufferBlock<BlockRolledBack>();
        private readonly ConcurrentDictionary<string, IServerStreamWriter<BlockRolledBack>> _subscriberBlockRolledBackMap = new ConcurrentDictionary<string, IServerStreamWriter<BlockRolledBack>>();

        public IndexerApiImpl(ConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        private string GetBlockchainsKey(string subscriber)
        {
            return $"Indexer:Subscriptions:{subscriber}:BlockchainTypes";
        }

        private string GetAddressesKey(string subscriber, string blockchainType)
        {
            return $"Indexer:Subscriptions:{subscriber}:BlockchainType:{blockchainType}:Addresses";
        }

        private string GetHashesKey(string subscriber, string blockchainType)
        {
            return $"Indexer:Subscriptions:{subscriber}:BlockchainType:{blockchainType}:TransactionHashes";
        }

        #region Subscriptions

        public override async Task<Empty> StartBlockchainObservation(BlockchainFilter request, ServerCallContext context)
        {
            var subscriber = context.Peer;
            await _redis.GetDatabase().SetAddAsync(GetBlockchainsKey(subscriber), request.BlockchainType);

            return new Empty();
        }

        public override async Task<Empty> EndBlockchainObservation(BlockchainFilter request, ServerCallContext context)
        {
            var subscriber = context.Peer;
            await _redis.GetDatabase().SetRemoveAsync(GetBlockchainsKey(subscriber), request.BlockchainType);

            return new Empty();
        }

        public override async Task<Empty> StartAddressObservation(AddressObservationFilter request, ServerCallContext context)
        {
            var subscriber = context.Peer;
            await _redis.GetDatabase().SetAddAsync(GetAddressesKey(subscriber, request.BlockchainType), request.Address);
            //await _redis.GetDatabase().SetAddAsync($"Indexer:Subscriptions:{subscriber}:BlockchainType:{request.BlockchainType}:AddressesObservationType", request.ObservationType.ToString());

            return new Empty();
        }

        public override async Task<Empty> EndAddressObservation(AddressObservationFilter request, ServerCallContext context)
        {
            var subscriber = context.Peer;
            await _redis.GetDatabase().SetRemoveAsync(GetAddressesKey(subscriber, request.BlockchainType), request.Address);

            return new Empty();
        }

        public override async Task<Empty> StartTransactionObservation(TransactionObservationFilter request, ServerCallContext context)
        {
            var subscriber = context.Peer;
            await _redis.GetDatabase().SetAddAsync(GetHashesKey(subscriber, request.BlockchainType), request.TransactionId);

            return new Empty();
        }


        public override async Task<Empty> EndTransactionObservation(TransactionObservationFilter request, ServerCallContext context)
        {
            var subscriber = context.Peer;
            await _redis.GetDatabase().SetRemoveAsync(GetHashesKey(subscriber, request.BlockchainType), request.TransactionId);

            return new Empty();
        }
        #endregion

        #region Streaming
        public override async Task GetTransactionExecutedEvents(Empty request, IServerStreamWriter<TransactionExecuted> responseStream, ServerCallContext context)
        {
            var subscriber = context.Peer;
            _subscriberExecutecTransactions[subscriber] = responseStream;

            while (_subscriberExecutecTransactions.ContainsKey(subscriber))
            {
                var @event = await _transactionExecutedEventsBuffer.ReceiveAsync();

                foreach (var subscription in _subscriberExecutecTransactions)
                {
                    if (await IsInterestedIn(subscription.Key, @event))
                        await subscription.Value.WriteAsync(@event);
                }
            }
        }

        public override async Task GetTransactionFailedEvents(Empty request, IServerStreamWriter<TransactionFailed> responseStream, ServerCallContext context)
        {
            var subscriber = context.Peer;
            _subscriberFailedTranscationsMap[subscriber] = responseStream;

            while (_subscriberFailedTranscationsMap.ContainsKey(subscriber))
            {
                var @event = await _transactionFailedEventsBuffer.ReceiveAsync();

                foreach (var subscription in _subscriberFailedTranscationsMap)
                {
                    if (await IsInterestedIn(subscription.Key, @event))
                        await subscription.Value.WriteAsync(@event);
                }
            }
        }

        public override async Task GetBlockRolledBackEvents(BlockchainFilter request, IServerStreamWriter<BlockRolledBack> responseStream, ServerCallContext context)
        {
            var subscriber = context.Peer;
            _subscriberBlockRolledBackMap[subscriber] = responseStream;

            while (_subscriberBlockRolledBackMap.ContainsKey(subscriber))
            {
                var @event = await _blockRolledBackEventsBuffer.ReceiveAsync();

                foreach (var subscription in _subscriberBlockRolledBackMap)
                {
                    if (await IsInterestedIn(subscription.Key, @event))
                        await subscription.Value.WriteAsync(@event);
                }
            }
        }

        public override async Task GetChainHeadExtendedEvents(BlockchainFilter request, IServerStreamWriter<ChainHeadExtended> responseStream, ServerCallContext context)
        {
            var subscriber = context.Peer;
            _subscriberChainHeadExtendedMap[subscriber] = responseStream;

            while (_subscriberChainHeadExtendedMap.ContainsKey(subscriber))
            {
                var @event = await _chainHeadExtendedEventsBuffer.ReceiveAsync();

                foreach (var subscription in _subscriberChainHeadExtendedMap)
                {
                    if (await IsInterestedIn(subscription.Key, @event))
                        await subscription.Value.WriteAsync(@event);
                    //System.InvalidOperationException: 'Only one write can be pending at a time'
                }
            }
        }

        public override async Task GetLastIrreversibleBlockUpdatedEvents(BlockchainFilter request, IServerStreamWriter<LastIrreversibleBlockUpdated> responseStream, ServerCallContext context)
        {
            var subscriber = context.Peer;
            _subscriberLastIrreversibleBlockUpdatedMap[subscriber] = responseStream;

            while (_subscriberLastIrreversibleBlockUpdatedMap.ContainsKey(subscriber))
            {
                var @event = await _lastIrreversibleBlockUpdatedEventsBuffer.ReceiveAsync();

                foreach (var subscription in _subscriberLastIrreversibleBlockUpdatedMap)
                {
                    if (await IsInterestedIn(subscription.Key, @event))
                        await subscription.Value.WriteAsync(@event);
                }
            }
        }

        #endregion

        #region Filtering
        private async Task<bool> IsInterestedIn(string client, TransactionExecuted @event)
        {
            // check by blockchain type
            if (await _redis.GetDatabase().SetContainsAsync(GetBlockchainsKey(client), @event.BlockchainType))
            {
                return true;
            }
            // check by transaction hash
            if (await _redis.GetDatabase().SetContainsAsync(GetHashesKey(client, @event.BlockchainType), @event.TransactionId))
            {
                return true;
            }
            // check by address
            foreach (var balanceUpdate in @event.BalanceUpdates)
            {
                if (await _redis.GetDatabase().SetContainsAsync(GetAddressesKey(client, @event.BlockchainType), balanceUpdate.AccountId.Address))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> IsInterestedIn(string client, TransactionFailed @event)
        {
            // check by blockchain type
            if (await _redis.GetDatabase().SetContainsAsync(GetBlockchainsKey(client), @event.BlockchainType))
            {
                return true;
            }
            // check by transaction hash
            if (await _redis.GetDatabase().SetContainsAsync(GetHashesKey(client, @event.BlockchainType), @event.TransactionId))
            {
                return true;
            }

            return false;
        }

        private async Task<bool> IsInterestedIn(string client, LastIrreversibleBlockUpdated @event)
        {
            // check by blockchain type
            if (await _redis.GetDatabase().SetContainsAsync(GetBlockchainsKey(client), @event.BlockchainType))
            {
                return true;
            }

            return false;
        }

        private async Task<bool> IsInterestedIn(string client, ChainHeadExtended @event)
        {
            // check by blockchain type
            if (await _redis.GetDatabase().SetContainsAsync(GetBlockchainsKey(client), @event.BlockchainType))
            {
                return true;
            }

            return false;
        }

        private async Task<bool> IsInterestedIn(string client, BlockRolledBack @event)
        {
            // check by blockchain type
            if (await _redis.GetDatabase().SetContainsAsync(GetBlockchainsKey(client), @event.BlockchainType))
            {
                return true;
            }

            return false;
        }
        #endregion

        #region Publishing
        public void Publish(TransactionExecutedEvent evt)
        {
            var result = new TransactionExecuted
            {
                BlockchainType = evt.BlockchainType,
                BlockId = evt.BlockId,
                BlockNumber = evt.BlockNumber,
                TransactionNumber = evt.TransactionNumber,
                TransactionId = evt.TransactionId,
                IsIrreversible = evt.IsIrreversible ?? true
            };
            result.BalanceUpdates.AddRange(evt.BalanceUpdates.Select(x => new BalanceUpdate
            {
                AccountId = new AccountId
                {
                    Asset = new Asset
                    {
                        Id = x.AccountId.Asset.Id,
                        Address = x.AccountId.Asset.Address
                    },
                    Address = x.AccountId.Address
                },
                NewBalance = x.NewBalance.ToString(),
                OldBalance = x.OldBalance.ToString(),

            }));
            result.Fees.AddRange(evt.Fees.Select(x => new Fee
            {
                Amount = x.Amount.ToString(),
                Asset = new Asset
                {
                    Address = x.Asset.Address,
                    Id = x.Asset.Id
                }
            }));
            _transactionExecutedEventsBuffer.Post(result);
        }

        public void Publish(TransactionFailedEvent evt)
        {
            var result = new TransactionFailed
            {
                BlockchainType = evt.BlockchainType,
                BlockId = evt.BlockId,
                BlockNumber = evt.BlockNumber,
                TransactionNumber = evt.TransactionNumber,
                TransactionId = evt.TransactionId,
                ErrorCode = evt.ErrorCode.ToString(),
                ErrorMessage = evt.ErrorMessage
            };
            if (evt.Fees != null)
            {
                result.Fees.AddRange(evt.Fees.Select(x => new Fee
                {
                    Amount = x.Amount.ToString(),
                    Asset = new Asset
                    {
                        Address = x.Asset.Address,
                        Id = x.Asset.Id
                    }
                }));
            }
            _transactionFailedEventsBuffer.Post(result);
        }

        public void Publish(LastIrreversibleBlockUpdatedEvent evt)
        {
            var result = new LastIrreversibleBlockUpdated
            {
                BlockchainType = evt.BlockchainType,
                BlockNumber = evt.BlockNumber,
                BlockId = evt.BlockId
            };
            _lastIrreversibleBlockUpdatedEventsBuffer.Post(result);
        }

        public void Publish(ChainHeadExtendedEvent evt)
        {
            var result = new ChainHeadExtended
            {
                BlockchainType = evt.BlockchainType,
                BlockNumber = evt.BlockNumber,
                BlockId = evt.BlockId,
                ChainHeadSequence = evt.ChainHeadSequence,
                PreviousBlockId = evt.PreviousBlockId
            };
            _chainHeadExtendedEventsBuffer.Post(result);
        }

        public void Publish(ChainHeadReducedEvent evt)
        {
            var result = new BlockRolledBack
            {
                BlockchainType = evt.BlockchainType,
                BlockNumber = evt.ToBlockNumber,
                BlockId = evt.ToBlockId,
                PreviousBlockId = evt.PreviousBlockId
            };
            _blockRolledBackEventsBuffer.Post(result);
        }

        #endregion
    }
}
