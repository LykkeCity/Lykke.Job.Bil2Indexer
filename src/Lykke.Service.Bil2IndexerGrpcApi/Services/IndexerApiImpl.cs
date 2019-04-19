using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Lykke.Service.Bil2IndexerGrpcApi.Services
{
    public class IndexerApiImpl : IndexerApi.IndexerApiBase
    {
        private readonly BufferBlock<TransactionExecuted> _buffer = new BufferBlock<TransactionExecuted>();

        private Dictionary<string, IServerStreamWriter<TransactionExecuted>> _subscriberWritersMap =
            new Dictionary<string, IServerStreamWriter<TransactionExecuted>>();

        public override async Task GetTransactionExecutedEvent(TransactionExecutedEventsFilter request, IServerStreamWriter<TransactionExecuted> responseStream, ServerCallContext context)
        {
            var subscriber = context.Peer;
            _subscriberWritersMap[subscriber] = responseStream;

            while (_subscriberWritersMap.ContainsKey(subscriber))
            {
                var @event = await _buffer.ReceiveAsync();
                foreach (var serverStreamWriter in _subscriberWritersMap.Values)
                {
                    await serverStreamWriter.WriteAsync(@event);
                }
            }
        }
        
        public void Publish(Lykke.Job.Bil2Indexer.Contract.Events.TransactionExecutedEvent evt)
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
            _buffer.Post(result);
        }
    }
}
