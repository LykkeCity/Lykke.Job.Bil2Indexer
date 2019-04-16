using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Settings;
using Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations;
using Lykke.Job.Bil2Indexer.VerifyingTool.BlockchainAdapters;
using Lykke.Job.Bil2Indexer.VerifyingTool.Reporting;
using Lykke.Job.Bil2Indexer.VerifyingTool.StartUpFolder;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.Sdk;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Common;
using Microsoft.Extensions.Configuration;
using BlockHeader = Lykke.Job.Bil2Indexer.Domain.BlockHeader;

namespace Lykke.Job.Bil2Indexer.VerifyingTool
{
    class Program
    {
        private static int _limit = 100;
        private static ReportingContext _reportingContext;

        static async Task Main(string[] args)
        {
            BigInteger fromBlock = BigInteger.Parse(args[0]);
            BigInteger toBlock = BigInteger.Parse(args[1]);
            string blockchainType = args[2];
            string dataBaseConnectionString = args[3];
            var restLength = args.Length - 4;
            string[] restArgs = new string[restLength];
            Array.Copy(args, 4, restArgs, 0, restLength);

            var logFactory = Logs.EmptyLogFactory.Instance;
            logFactory.AddConsole();
            var taskWithIndexer = Task.Run(() =>
            {
                return LykkeStarter.Start<StartupInMemory>(true, 5001);
            });
            await Task.Delay(15000);
            IBlockHeadersRepository blockHeadersRepository =
                StartupInMemory.ServiceProvider.GetRequiredService<IBlockHeadersRepository>();
            ITransactionsRepository transactionsRepository =
                StartupInMemory.ServiceProvider.GetRequiredService<ITransactionsRepository>();

            var configurationRoot = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var settings = configurationRoot.LoadSettings<AppSettings>(options =>
            {
                options.SetConnString(x => x.SlackNotifications?.AzureQueue.ConnectionString);
                options.SetQueueName(x => x.SlackNotifications?.AzureQueue.QueueName);
                options.SenderName = $"{AppEnvironment.Name} {AppEnvironment.Version}";
            });

            var appSettings = settings;
            var blockchainIntegrationSettings = appSettings
                .CurrentValue
                .BlockchainIntegrations
                .FirstOrDefault(x => x.Type == blockchainType);

            BlockHeader header = null;

            do
            {
                header = await blockHeadersRepository.GetOrDefaultAsync(blockchainType, 19);

                if (header != null)
                {
                    break;
                }
                else
                {
                    await Task.Delay(10000);
                }

            } while (true);

            var adapter = InitAdapter(blockchainType, restArgs);
            _reportingContext = new ReportingContext("report_" + blockchainType);
            //IServiceCollection serviceCollection = new ServiceCollection();
            //Lykke.Bil2.Ripple.Client.ServiceCollectionExtensions.AddRippleClient(serviceCollection, nodeUrl);
            //var rippleApi = serviceCollection.BuildServiceProvider().GetRequiredService<IRippleApi>();
            using (_reportingContext)
            {
                await MakeReportAsync(fromBlock, 
                    toBlock, 
                    blockHeadersRepository, 
                    blockchainType, 
                    adapter, 
                    blockchainIntegrationSettings.Capabilities.TransferModel, 
                    transactionsRepository);
            }

            //for (var i = fromBlock; i <= toBlock; i++)
            //{
            //    var block = await rippleApi.Post(new BinaryLedgerWithTransactionsRequest((uint)fromBlock));
            //    var indexedBlock = await blockHeadersRepository.GetOrDefaultAsync(blockchainType, (long)fromBlock);
            //    //block.Result.Ledger.Transactions[0].Parse().

            //    //Indexed block properties to compare
            //    //indexedBlock.BlockchainType;
            //    //indexedBlock.Id;
            //    //indexedBlock.IsExecuted;
            //    //indexedBlock.Number;
            //    //indexedBlock.PreviousBlockId;
            //    //indexedBlock.TransactionsCount;
            //    //indexedBlock.State;

            //    #region Check Block Indexation 

            //    var blockResult = block.Result;
            //    var ledger = blockResult.Ledger.Parse();
            //    var transactionsCount = ledger.Transactions.Length;
            //    var ledgerHash = blockResult.LedgerHash;
            //    var ledgerIndex = blockResult.LedgerIndex;
            //    var ledgerParentHash = ledger.ParentHash;

            //    AssertEqual(blockchainType, indexedBlock.BlockchainType);
            //    AssertEqual(indexedBlock.Id, ledgerHash);
            //    AssertEqual(indexedBlock.IsExecuted, true);
            //    AssertEqual((int)indexedBlock.State, (int)BlockState.Executed);
            //    AssertEqual(indexedBlock.Number, ledgerIndex);
            //    AssertEqual(indexedBlock.PreviousBlockId, ledgerParentHash);
            //    AssertEqual(indexedBlock.TransactionsCount, transactionsCount);

            //    #endregion

            //    #region Check Transaction Indexation

            //    string continuation = null;
            //    List<TransferAmountTransactionExecutedEvent> transfers = new List<TransferAmountTransactionExecutedEvent>(100);

            //    do
            //    {
            //        var paginationResponse = await
            //            transactionsRepository.GetTransferAmountTransactionsOfBlockAsync(
            //                blockchainType,
            //                indexedBlock.Id,
            //                continuation);

            //        continuation = paginationResponse.Continuation;

            //        if (paginationResponse?.Items != null &&
            //            paginationResponse.Items.Any())
            //        {
            //            transfers.AddRange(paginationResponse.Items);
            //        }

            //    } while (!string.IsNullOrEmpty(continuation));

            //    var orderedTransactions = transfers.OrderBy(x => x.TransactionNumber).ToArray();

            //    AssertEqual(transfers.Count, transactionsCount);

            //    var transactions = ledger.Transactions;
            //    for (int j = 0; j < transactions.Length; j++)
            //    {
            //        //For Coin Transfers
            //        //orderedTransactions[i].TransactionNumber;
            //        //orderedTransactions[i].BlockId;
            //        //orderedTransactions[i].Fees;
            //        //orderedTransactions[i].IsIrreversible;
            //        //orderedTransactions[i].SpentCoins;
            //        //orderedTransactions[i].TransactionId;
            //        //orderedTransactions[i].ReceivedCoins;

            //        //For Amount Transfers
            //        //orderedTransactions[i].TransactionNumber;
            //        //orderedTransactions[i].BlockId;
            //        //orderedTransactions[i].Fees;
            //        //orderedTransactions[i].IsIrreversible;
            //        //orderedTransactions[i].BalanceChanges;
            //        //orderedTransactions[i].TransactionId;

            //        // Ripple Transaction
            //        //transactions[i].Account;
            //        //transactions[i].Amount;
            //        //transactions[i].Destination;
            //        //transactions[i].DestinationTag;
            //        //transactions[i].Fee;
            //        //transactions[i].Flags;
            //        //transactions[i].LastLedgerSequence;
            //        //transactions[i].TransactionType;
            //        //transactions[i].Sequence;

            //        AssertEqual((uint)orderedTransactions[j].TransactionNumber, transactions[j].Sequence);
            //        AssertEqual(orderedTransactions[j].TransactionId, transactions[j].TransactionType);
            //        //AssertEqual(orderedTransactions[j].TransactionId, transactions[j].);

            //        var transfer = orderedTransactions[i]
            //            .BalanceChanges
            //            .FirstOrDefault(x => x.Address == transactions[j].Account &&
            //                                 x.Value. transactions[j].Amount.);
            //        //AssertEqual(orderedTransactions[j].Fees, transactions[j].Fee);
            //    }


            //    #endregion
            //}
        }

        private static async Task MakeReportAsync(BigInteger fromBlock, BigInteger toBlock,
            IBlockHeadersRepository blockHeadersRepository, string blockchainType, IBlockchainVerifierAdapter adapter,
            BlockchainTransferModel transferModel, ITransactionsRepository transactionsRepository)
        {
            _reportingContext.StartListScope();

            for (var currentBlockNumber = fromBlock; currentBlockNumber <= toBlock; currentBlockNumber++)
            {
                var indexedBlock = await blockHeadersRepository.GetOrDefaultAsync(blockchainType, (long) currentBlockNumber);
                var realBlock = await adapter.GetBlockAsync(currentBlockNumber);

                #region Check Block Indexation 

                if (indexedBlock == null)
                {
                    //TODO:
                    AssertNotNull(indexedBlock);
                    continue;
                }

                AssertBlockHeaders(indexedBlock, realBlock);

                #endregion

                #region Check Transaction Indexation

                if (transferModel == BlockchainTransferModel.Coins)
                {
                    string continuation = null;
                    List<TransferCoinsTransactionExecutedEvent> transfers =
                        new List<TransferCoinsTransactionExecutedEvent>(100);

                    do
                    {
                        var paginationResponse = await
                            transactionsRepository.GetTransferCoinsTransactionsOfBlockAsync(
                                blockchainType,
                                indexedBlock.Id,
                                _limit,
                                continuation);

                        continuation = paginationResponse.Continuation;

                        if (paginationResponse?.Items != null &&
                            paginationResponse.Items.Any())
                        {
                            transfers.AddRange(paginationResponse.Items);
                        }
                    } while (!string.IsNullOrEmpty(continuation));

                    var failedTransfers =
                        await GetFailedEventsForBlockAsync(transactionsRepository, blockchainType, indexedBlock);

                    var orderedTransactions = transfers.OrderBy(x => x.TransactionNumber).ToArray();
                    var (realCoinTransfers, realFailedEvents) =
                        await adapter.GetTransactionsForBlockAsync(currentBlockNumber);
                    var orderedRealCoinTransfers = realCoinTransfers.OrderBy(x => x.TransactionNumber).ToArray();
                    var orderedRealFailedEvents = realFailedEvents.OrderBy(x => x.TransactionNumber).ToArray();

                    AssertEqual(
                        orderedTransactions.Length,
                        orderedRealCoinTransfers.Count(),
                        nameof(realCoinTransfers));
                    AssertEqual(failedTransfers.Count,
                        orderedRealFailedEvents.Count(), 
                        nameof(realFailedEvents));

                    _reportingContext.StartListScope("coinTransfers");

                    for (int j = 0; j < orderedRealCoinTransfers.Length; j++)
                    {
                        var orderedTransactionIndexed = orderedTransactions[j];
                        var orderedTransactionReal = orderedRealCoinTransfers[j];

                        AssertCoinTransfers(orderedTransactionIndexed, orderedTransactionReal);

                        var spentCoinsReal = orderedTransactionReal.SpentCoins.ToArray();
                        var spentCoinsIndexed = orderedTransactionIndexed.SpentCoins.ToArray();

                        AssertSpentCoins(spentCoinsReal, spentCoinsIndexed);

                        var receivedCoinsReal = orderedTransactionReal.ReceivedCoins.ToArray();
                        var receivedCoinsIndexed = orderedTransactionIndexed.ReceivedCoins.ToArray();

                        AssertReceivedCoins(receivedCoinsReal, receivedCoinsIndexed);

                        var feesReal = orderedTransactionReal.Fees?.ToArray();
                        var feesIndexed = orderedTransactionIndexed.Fees?.ToArray();

                        AssertFees(feesReal, feesIndexed);

                        _reportingContext.EndScope();
                    }

                    _reportingContext.EndScope();
                }
                else if (transferModel == BlockchainTransferModel.Amount)
                {
                    //for (int j = 0; j < orderedTransactions.Length; j++)
                    //{
                    //    //For Amount Transfers
                    //    //orderedTransactions[i].TransactionNumber;
                    //    //orderedTransactions[i].BlockId;
                    //    //orderedTransactions[i].Fees;
                    //    //orderedTransactions[i].IsIrreversible;
                    //    //orderedTransactions[i].BalanceChanges;
                    //    //orderedTransactions[i].TransactionId;

                    //    // Ripple Transaction
                    //    //transactions[i].Account;
                    //    //transactions[i].Amount;
                    //    //transactions[i].Destination;
                    //    //transactions[i].DestinationTag;
                    //    //transactions[i].Fee;
                    //    //transactions[i].Flags;
                    //    //transactions[i].LastLedgerSequence;
                    //    //transactions[i].TransactionType;
                    //    //transactions[i].Sequence;

                    //    //AssertEqual((uint) orderedTransactions[j].TransactionNumber, transactions[j].Sequence);
                    //    //AssertEqual(orderedTransactions[j].TransactionId, transactions[j].TransactionType);
                    //    ////AssertEqual(orderedTransactions[j].TransactionId, transactions[j].);

                    //    //var transfer = orderedTransactions[i]
                    //    //    .BalanceChanges
                    //    //    .FirstOrDefault(x => x.Address == transactions[j].Account &&
                    //    //                         x.Value.transactions[j].Amount.);
                    //    //AssertEqual(orderedTransactions[j].Fees, transactions[j].Fee);
                    //}
                }
                else
                {
                    //Not Supported
                }

                #endregion

                await _reportingContext.FlushAsync();
            }

            _reportingContext.EndScope();
        }

        private static void AssertFees(Fee[] feesReal, Fee[] feesIndexed)
        {
            _reportingContext.StartListScope("fees");

            for (int feesIndex = 0; feesIndex < feesReal?.Length; feesIndex++)
            {
                _reportingContext.StartScope();

                var feeIndexed = feesIndexed[feesIndex];
                var feeReal = feesReal[feesIndex];

                AssertEqual(feeIndexed.Amount, feeReal.Amount, nameof(feeReal.Amount));
                AssertEqual(feeIndexed.Asset, feeReal.Asset, nameof(feeReal.Asset));

                _reportingContext.EndScope();
            }
        }

        private static void AssertReceivedCoins(ReceivedCoin[] receivedCoinsReal, ReceivedCoin[] receivedCoinsIndexed)
        {
            _reportingContext.StartListScope("receivedCoins");

            for (int receivedCoinIndex = 0; receivedCoinIndex < receivedCoinsReal.Length; receivedCoinIndex++)
            {
                _reportingContext.StartScope();

                var receivedCoinIndexed = receivedCoinsIndexed[receivedCoinIndex];
                var receivedCoinReal = receivedCoinsReal[receivedCoinIndex];

                AssertEqual(receivedCoinIndexed.CoinNumber, receivedCoinReal.CoinNumber, 
                    nameof(receivedCoinReal.CoinNumber));
                AssertEqual(receivedCoinIndexed.Address, receivedCoinReal.Address, 
                    nameof(receivedCoinReal.Address));
                AssertEqual(receivedCoinIndexed.AddressNonce ?? 0, receivedCoinReal.AddressNonce ?? 0, 
                    nameof(receivedCoinReal.AddressNonce));
                AssertEqual(receivedCoinIndexed.Address, receivedCoinReal.Address, 
                    nameof(receivedCoinReal.Address));
                AssertEqual((int?) receivedCoinIndexed.AddressTagType ?? 0,
                    (int?) receivedCoinReal.AddressTagType ?? 0, nameof(receivedCoinReal.AddressTagType));
                AssertEqual(receivedCoinIndexed.Value, receivedCoinReal.Value, nameof(receivedCoinReal.Value));
                AssertEqual(receivedCoinIndexed.Asset, receivedCoinReal.Asset, nameof(receivedCoinReal.Asset));

                _reportingContext.EndScope();
            }

            _reportingContext.EndScope();
        }

        private static void AssertSpentCoins(CoinId[] spentCoinsReal, CoinId[] spentCoinsIndexed)
        {
            _reportingContext.StartListScope("spentCoin");

            for (int spentCoinIndex = 0; spentCoinIndex < spentCoinsReal.Length; spentCoinIndex++)
            {
                _reportingContext.StartScope();

                var spentCoinIndexed = spentCoinsIndexed[spentCoinIndex];
                var spentCoinReal = spentCoinsReal[spentCoinIndex];

                AssertEqual(spentCoinIndexed.CoinNumber, 
                    spentCoinReal.CoinNumber, 
                    nameof(spentCoinReal.CoinNumber));
                AssertEqual(spentCoinIndexed.TransactionId, 
                    spentCoinReal.TransactionId, 
                    nameof(spentCoinReal.TransactionId));

                _reportingContext.EndScope();
            }

            _reportingContext.EndScope();
        }

        private static void AssertCoinTransfers(TransferCoinsTransactionExecutedEvent orderedTransactionIndexed,
            TransferCoinsTransactionExecutedEvent orderedTransactionReal)
        {
            AssertEqual(orderedTransactionIndexed.TransactionNumber, orderedTransactionReal.TransactionNumber,
                nameof(orderedTransactionReal.TransactionNumber));
            AssertEqual(orderedTransactionIndexed.TransactionId, orderedTransactionReal.TransactionId,
                nameof(orderedTransactionReal.TransactionId));
            AssertEqual(orderedTransactionIndexed.BlockId, orderedTransactionReal.BlockId,
                nameof(orderedTransactionReal.BlockId));
            AssertEqual(orderedTransactionIndexed.IsIrreversible ?? false,
                orderedTransactionReal.IsIrreversible ?? false,
                nameof(orderedTransactionReal.IsIrreversible));
        }

        private static void AssertBlockHeaders(BlockHeader indexedBlock, BlockHeader realBlock)
        {
            _reportingContext.StartScope("blocksHeader");

            AssertEqual(indexedBlock.BlockchainType, realBlock.BlockchainType, nameof(realBlock.BlockchainType));
            AssertEqual(indexedBlock.Id, realBlock.Id, nameof(realBlock.Id));
            AssertEqual(indexedBlock.IsExecuted, true, nameof(realBlock.IsExecuted));
            AssertEqual((int)indexedBlock.State, (int)BlockState.Executed, nameof(realBlock.State));
            AssertEqual(indexedBlock.Number, realBlock.Number, nameof(realBlock.Number));
            AssertEqual(indexedBlock.PreviousBlockId, realBlock.PreviousBlockId, nameof(realBlock.PreviousBlockId));
            AssertEqual(indexedBlock.TransactionsCount, realBlock.TransactionsCount, nameof(realBlock.TransactionsCount));
            AssertEqual(indexedBlock.CanBeExecuted, realBlock.CanBeExecuted, nameof(realBlock.CanBeExecuted));
            AssertEqual(indexedBlock.MinedAt, realBlock.MinedAt, nameof(realBlock.MinedAt));
            AssertEqual(indexedBlock.Version, realBlock.Version, nameof(realBlock.Version));
            AssertEqual(indexedBlock.Size, realBlock.Size, nameof(realBlock.Size));

            _reportingContext.EndScope();
        }

        private static void AssertNotNull(object obj)
        {
            if (obj == null)
            {
                //TODO:REPORT
            }
        }

        private static async Task<List<TransactionFailedEvent>> GetFailedEventsForBlockAsync(
            ITransactionsRepository transactionsRepository, string blockchainType,
            BlockHeader indexedBlock)
        {
            string continuation = null;
            var failedTransfers = new List<TransactionFailedEvent>(100);

            do
            {
                var paginationResponse = await
                    transactionsRepository.GetFailedTransactionsOfBlockAsync(
                        blockchainType,
                        indexedBlock.Id,
                        _limit,
                        continuation);

                continuation = paginationResponse.Continuation;

                if (paginationResponse?.Items != null &&
                    paginationResponse.Items.Any())
                {
                    failedTransfers.AddRange(paginationResponse.Items);
                }
            } while (!string.IsNullOrEmpty(continuation));

            return failedTransfers;
        }

        private static IBlockchainVerifierAdapter InitAdapter(string blockchainType, string[] args)
        {
            BlockchainVerifierAdapterFactory factory = new BlockchainVerifierAdapterFactory();
            return factory.GetAdapter(blockchainType, args);
        }

        private static void AssertEqual<T>(T indexedField, T realField, string fieldName = null) where T : IComparable<T>
        {
            if (indexedField?.CompareTo(realField) != 0)
            {
                _reportingContext.CurrentReportObject[fieldName] = new AssertObject<T>()
                {
                    Indexed = indexedField,
                    Real = realField
                };
            }
        }
    }
}
