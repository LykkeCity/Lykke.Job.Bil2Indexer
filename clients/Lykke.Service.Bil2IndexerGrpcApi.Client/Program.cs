using Bil2.Indexer;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.Bil2IndexerGrpcApi.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var channel = new Channel("localhost:5100", ChannelCredentials.Insecure);
            //var channel = new Channel("indexer-grpc-api.bil2.svc.cluster.local:5100", ChannelCredentials.Insecure);
            var subscriber = new Subsriber(new IndexerApi.IndexerApiClient(channel));

            Task.Run(async () =>
            {
                await subscriber.Subscribe();
                Task.WaitAll(new[]
                {
                    subscriber.GetTransactionExecutedEvents(),
                    subscriber.GetTransactionFailedEvents(),
                    subscriber.GetLastIrreversibleBlockUpdatedEvents(),
                    subscriber.GetBlockRolledBackEvents(),
                    subscriber.GetChainHeadExtendedEvents()
                });
            }).GetAwaiter();

            Console.WriteLine("Hit key to unsubscribe");
            Console.ReadLine();

            subscriber.Unsubscribe().Wait();
            Console.WriteLine("Unsubscribed...");

            Console.WriteLine("Hit key to exit...");
            Console.ReadLine();

        }

        public class Subsriber
        {
            private readonly IndexerApi.IndexerApiClient _pubSubClient;
            public Subsriber(IndexerApi.IndexerApiClient pubSubClient)
            {
                _pubSubClient = pubSubClient;
            }

            public async Task Subscribe()
            {
                var filter0 = new BlockchainFilter { BlockchainType = "Ripple"};
                using (var call = _pubSubClient.StartBlockchainObservationAsync(filter0))
                {
                    await call.ResponseAsync;
                }

                var filter1 = new AddressObservationFilter { BlockchainType = "Ripple", Address = "123", ObservationType = AddressObservationFilter.Types.ObservationType.IncomingTransactions };
                using (var call = _pubSubClient.StartAddressObservationAsync(filter1))
                {
                    await call.ResponseAsync;
                }

                var filter2 = new TransactionObservationFilter { BlockchainType = "Ripple", TransactionId = "456" };
                using (var call = _pubSubClient.StartTransactionObservationAsync(filter2))
                {
                    await call.ResponseAsync;
                }
            }

            public async Task Unsubscribe()
            {
                var filter0 = new BlockchainFilter { BlockchainType = "Ripple" };
                using (var call = _pubSubClient.EndBlockchainObservationAsync(filter0))
                {
                    await call.ResponseAsync;
                }

                var filter1 = new AddressObservationFilter { BlockchainType = "Ripple", Address = "123", ObservationType = AddressObservationFilter.Types.ObservationType.IncomingTransactions };
                using (var call = _pubSubClient.EndAddressObservationAsync(filter1))
                {
                    await call.ResponseAsync;
                }

                var filter2 = new TransactionObservationFilter { BlockchainType = "Ripple", TransactionId = "456" };
                using (var call = _pubSubClient.EndTransactionObservationAsync(filter2))
                {
                    await call.ResponseAsync;
                }
            }

            public async Task GetTransactionExecutedEvents()
            {
                using (var call = _pubSubClient.GetTransactionExecutedEvents(new Empty()))
                {
                    //Receive
                    var responseReaderTask = Task.Run(async () =>
                    {
                        while (await call.ResponseStream.MoveNext())
                        {
                            Console.WriteLine("TransactionExecuted event received: " + call.ResponseStream.Current);
                        }
                    });

                    await responseReaderTask;
                }
            }

            public async Task GetTransactionFailedEvents()
            {
                using (var call = _pubSubClient.GetTransactionFailedEvents(new Empty()))
                {
                    //Receive
                    var responseReaderTask = Task.Run(async () =>
                    {
                        while (await call.ResponseStream.MoveNext())
                        {
                            Console.WriteLine("TransactionFailed event received: " + call.ResponseStream.Current);
                        }
                    });

                    await responseReaderTask;
                }
            }

            public async Task GetChainHeadExtendedEvents()
            {
                using (var call = _pubSubClient.GetChainHeadExtendedEvents(new BlockchainFilter()))
                {
                    //Receive
                    var responseReaderTask = Task.Run(async () =>
                    {
                        while (await call.ResponseStream.MoveNext())
                        {
                            Console.WriteLine("ChainHeadExtended event received: " + call.ResponseStream.Current);
                        }
                    });

                    await responseReaderTask;
                }
            }

            public async Task GetLastIrreversibleBlockUpdatedEvents()
            {
                using (var call = _pubSubClient.GetLastIrreversibleBlockUpdatedEvents(new BlockchainFilter()))
                {
                    //Receive
                    var responseReaderTask = Task.Run(async () =>
                    {
                        while (await call.ResponseStream.MoveNext())
                        {
                            Console.WriteLine("LastIrreversibleBlockUpdated event received: " + call.ResponseStream.Current);
                        }
                    });

                    await responseReaderTask;
                }
            }

            public async Task GetBlockRolledBackEvents()
            {
                using (var call = _pubSubClient.GetBlockRolledBackEvents(new BlockchainFilter()))
                {
                    //Receive
                    var responseReaderTask = Task.Run(async () =>
                    {
                        while (await call.ResponseStream.MoveNext())
                        {
                            Console.WriteLine("BlockRolledBack event received: " + call.ResponseStream.Current);
                        }
                    });

                    await responseReaderTask;
                }
            }
        }
    }
}
