using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Bil2.Ripple.Client;
using Lykke.Bil2.Ripple.Client.Api.Ledger;
using Lykke.Job.Bil2Indexer.AzureRepositories;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Logs.Loggers.LykkeConsole;
using Microsoft.Extensions.DependencyInjection;
using System.Numerics;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Job.Bil2Indexer.Domain;

namespace Lykke.Job.Bil2Indexer.VerifyingTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            BigInteger fromBlock = BigInteger.Parse(args[0]);
            BigInteger toBlock = BigInteger.Parse(args[1]);
            string blockchainType = args[2];
            string nodeUrl = args[4];

            var logFactory = Logs.EmptyLogFactory.Instance;
            logFactory.AddConsole();

            IBalanceActionsRepository balanceActionsRepository = new InMemoryBalanceActionsRepository();
            IBlockHeadersRepository blockHeadersRepository = new InMemoryBlockHeadersRepository(logFactory);
            ICoinsRepository coinsRepository = new InMemoryCoinsRepository();
            ICrawlersRepository crawlersRepository = new InMemoryCrawlersRepository(logFactory);
            ITransactionsRepository transactionsRepository = new InMemoryTransactionsRepository(logFactory);
            IChainHeadsRepository chainHeadsRepository = new InMemoryChainHeadsRepository(logFactory);
            IFeeEnvelopesRepository feeEnvelopesRepository = new InMemoryFeeEnvelopesRepository();

            IServiceCollection serviceCollection = new ServiceCollection();
            Lykke.Bil2.Ripple.Client.ServiceCollectionExtensions.AddRippleClient(serviceCollection, nodeUrl);
            var rippleApi = serviceCollection.BuildServiceProvider().GetRequiredService<IRippleApi>();

            for (var i = fromBlock; i <= toBlock; i++)
            {
                var block = await rippleApi.Post(new BinaryLedgerWithTransactionsRequest((uint)fromBlock));
                var indexedBlock = await blockHeadersRepository.GetOrDefaultAsync(blockchainType, (long)fromBlock);
                //block.Result.Ledger.Transactions[0].Parse().

                //Indexed block properties to compare
                //indexedBlock.BlockchainType;
                //indexedBlock.Id;
                //indexedBlock.IsExecuted;
                //indexedBlock.Number;
                //indexedBlock.PreviousBlockId;
                //indexedBlock.TransactionsCount;
                //indexedBlock.State;

                #region Check Block Indexation

                var blockResult = block.Result;
                var ledger = blockResult.Ledger.Parse();
                var transactionsCount = ledger.Transactions.Length;
                var ledgerHash = blockResult.LedgerHash;
                var ledgerIndex = blockResult.LedgerIndex;
                var ledgerParentHash = ledger.ParentHash;

                AssertEqual(blockchainType, indexedBlock.BlockchainType);
                AssertEqual(indexedBlock.Id, ledgerHash);
                AssertEqual(indexedBlock.IsExecuted, true);
                AssertEqual((int)indexedBlock.State, (int)BlockState.Executed);
                AssertEqual(indexedBlock.Number, ledgerIndex);
                AssertEqual(indexedBlock.PreviousBlockId, ledgerParentHash);
                AssertEqual(indexedBlock.TransactionsCount, transactionsCount);

                #endregion

                #region Check Transaction Indexation

                string continuation = null;
                List<TransferCoinsTransactionExecutedEvent> transfers = new List<TransferCoinsTransactionExecutedEvent>(100);

                do
                {
                    var paginationResponse = await
                        transactionsRepository.GetTransferCoinsTransactionsOfBlockAsync(
                            blockchainType,
                            indexedBlock.Id,
                            continuation);

                    continuation = paginationResponse.Continuation;

                    if (paginationResponse?.Items != null &&
                        paginationResponse.Items.Any())
                    {
                        transfers.AddRange(paginationResponse.Items);
                    }

                } while (!string.IsNullOrEmpty(continuation));

                var orderedTransactions = transfers.OrderBy(x => x.TransactionNumber).ToArray();

                AssertEqual(transfers.Count, transactionsCount);

                var transactions = ledger.Transactions;
                for (int j = 0; j < transactions.Length; j++)
                {
                    //orderedTransactions[i].TransactionNumber;
                    //orderedTransactions[i].BlockId;
                    //orderedTransactions[i].Fees;
                    //orderedTransactions[i].IsIrreversible;
                    //orderedTransactions[i].SpentCoins;
                    //orderedTransactions[i].TransactionId;
                    //orderedTransactions[i].ReceivedCoins;

                    //transactions[i].Account;
                    //transactions[i].Amount;
                    //transactions[i].Destination;
                    //transactions[i].DestinationTag;
                    //transactions[i].Fee;
                    //transactions[i].Flags;
                    //transactions[i].LastLedgerSequence;
                    //transactions[i].TransactionType;
                    //transactions[i].Sequence;

                    //AssertEqual(,);
                }
                

                #endregion
            }
        }

        private static bool AssertEqual<T>(T obj1, T obj2) where T : IComparable<T>
        {
            return obj1?.CompareTo(obj2) == 0;
        }
    }
}
