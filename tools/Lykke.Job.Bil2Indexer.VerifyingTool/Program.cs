﻿using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Job.Bil2Indexer.AzureRepositories;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations;
using Lykke.Job.Bil2Indexer.VerifyingTool.Reporting;
using Lykke.Logs.Loggers.LykkeConsole;
using NBitcoin;
using QBitNinja.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using QBitNinja.Client.Models;
using BlockHeader = Lykke.Job.Bil2Indexer.Domain.BlockHeader;

namespace Lykke.Job.Bil2Indexer.VerifyingTool
{
    class Program
    {
        private static int _limit = 100;

        static async Task Main(string[] args)
        {
            BigInteger fromBlock = BigInteger.Parse(args[0]);
            BigInteger toBlock = BigInteger.Parse(args[1]);
            string blockchainType = args[2];
            string dataBaseConnectionString = args[3];
            var restLength = args.Length - 3;
            string[] restArgs = new string[restLength];
            Array.Copy(args, 4, restArgs, 0, restLength);

            var logFactory = Logs.EmptyLogFactory.Instance;
            logFactory.AddConsole();

            IBalanceActionsRepository balanceActionsRepository = new InMemoryBalanceActionsRepository();
            IBlockHeadersRepository blockHeadersRepository = new InMemoryBlockHeadersRepository(logFactory);
            ICoinsRepository coinsRepository = new InMemoryCoinsRepository();
            ICrawlersRepository crawlersRepository = new InMemoryCrawlersRepository(logFactory);
            ITransactionsRepository transactionsRepository = new InMemoryTransactionsRepository(logFactory);
            IChainHeadsRepository chainHeadsRepository = new InMemoryChainHeadsRepository(logFactory);
            IFeeEnvelopesRepository feeEnvelopesRepository = new InMemoryFeeEnvelopesRepository();

            var blockchainIntegrationSettings = new BlockchainIntegrationSettings()
            {
                Capabilities = new BlockchainCapabilitiesSettings()
                {
                    FirstBlockNumber = 0,
                    TransferModel = BlockchainTransferModel.Coins
                },
                Type = "Bitcoin",
                Indexer = new BlockchainIndexerSettings()
                {

                }
            };
            var adapter = InitAdapter(blockchainType, restArgs);

            //IServiceCollection serviceCollection = new ServiceCollection();
            //Lykke.Bil2.Ripple.Client.ServiceCollectionExtensions.AddRippleClient(serviceCollection, nodeUrl);
            //var rippleApi = serviceCollection.BuildServiceProvider().GetRequiredService<IRippleApi>();
            using (ReportingContext reportingContext = new ReportingContext(""))
            {
                for (var currentBlockNumber = fromBlock; currentBlockNumber <= toBlock; currentBlockNumber++)
                {
                    var indexedBlock = await blockHeadersRepository.GetOrDefaultAsync(blockchainType, (long) currentBlockNumber);
                    var realBlock = await adapter.GetBlockAsync(currentBlockNumber);

                    #region Check Block Indexation 

                    AssertEqual(indexedBlock.BlockchainType, realBlock.BlockchainType);
                    AssertEqual(indexedBlock.Id, realBlock.Id);
                    AssertEqual(indexedBlock.IsExecuted, true);
                    AssertEqual((int) indexedBlock.State, (int) BlockState.Executed);
                    AssertEqual(indexedBlock.Number, realBlock.Number);
                    AssertEqual(indexedBlock.PreviousBlockId, realBlock.PreviousBlockId);
                    AssertEqual(indexedBlock.TransactionsCount, realBlock.TransactionsCount);
                    AssertEqual(indexedBlock.TransactionsCount, realBlock.TransactionsCount);

                    #endregion

                    #region Check Transaction Indexation

                    if (blockchainIntegrationSettings.Capabilities.TransferModel == BlockchainTransferModel.Coins)
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

                        AssertEqual(orderedTransactions.Length, realCoinTransfers.Count());
                        AssertEqual(failedTransfers.Count, realFailedEvents.Count());


                        for (int j = 0; j < orderedRealCoinTransfers.Length; j++)
                        {
                            var orderedTransactionIndexed = orderedTransactions[j];
                            var orderedTransactionReal = orderedRealCoinTransfers[j];

                            AssertEqual(orderedTransactionIndexed.TransactionNumber, orderedTransactionReal.TransactionNumber);
                            AssertEqual(orderedTransactionIndexed.TransactionId, orderedTransactionReal.TransactionId);
                            AssertEqual(orderedTransactionIndexed.BlockId, orderedTransactionReal.BlockId);
                            AssertEqual(orderedTransactionIndexed.IsIrreversible ?? false, orderedTransactionReal.IsIrreversible ?? false);

                            var spentCoinsReal = orderedTransactionReal.SpentCoins.ToArray();
                            var spentCoinsIndexed = orderedTransactionIndexed.SpentCoins.ToArray();

                            for (int spentCoinIndex = 0; spentCoinIndex < spentCoinsReal.Length; spentCoinIndex++)
                            {
                                var spentCoinIndexed = spentCoinsIndexed[spentCoinIndex];
                                var spentCoinReal = spentCoinsReal[spentCoinIndex];

                                AssertEqual(spentCoinIndexed.CoinNumber, spentCoinReal.CoinNumber);
                                AssertEqual(spentCoinIndexed.TransactionId, spentCoinReal.TransactionId);
                            }

                            var receivedCoinsReal = orderedTransactionReal.ReceivedCoins.ToArray();
                            var receivedCoinsIndexed = orderedTransactionIndexed.ReceivedCoins.ToArray();

                            for (int receivedCoinIndex = 0; receivedCoinIndex < receivedCoinsReal.Length; receivedCoinIndex++)
                            {
                                var receivedCoinIndexed = receivedCoinsIndexed[receivedCoinIndex];
                                var receivedCoinReal = receivedCoinsReal[receivedCoinIndex];

                                AssertEqual(receivedCoinIndexed.CoinNumber, receivedCoinReal.CoinNumber);
                                AssertEqual(receivedCoinIndexed.Address, receivedCoinReal.Address);
                                AssertEqual(receivedCoinIndexed.AddressNonce ?? 0, receivedCoinReal.AddressNonce ?? 0);
                                AssertEqual(receivedCoinIndexed.Address, receivedCoinReal.Address);
                                AssertEqual((int?)receivedCoinIndexed.AddressTagType ?? 0, 
                                    (int?)receivedCoinReal.AddressTagType ?? 0);
                                AssertEqual(receivedCoinIndexed.Value, receivedCoinReal.Value);
                                AssertEqual(receivedCoinIndexed.Asset, receivedCoinReal.Asset);
                            }

                            var feesReal = orderedTransactionReal.Fees.ToArray();
                            var feesIndexed = orderedTransactionIndexed.Fees.ToArray();

                            for (int feesIndex = 0; feesIndex < feesReal.Length; feesIndex++)
                            {
                                var feeIndexed = feesIndexed[feesIndex];
                                var feeReal = feesReal[feesIndex];

                                AssertEqual(feeIndexed.Amount, feeReal.Amount);
                                AssertEqual(feeIndexed.Asset, feeReal.Asset);
                            }
                        }
                    }
                    else if (blockchainIntegrationSettings.Capabilities.TransferModel ==BlockchainTransferModel.Amount)
                    {
                        //for (int j = 0; j < orderedTransactions.Length; j++)
                        //{
                        //    //For Coin Transfers
                        //    //orderedTransactions[i].TransactionNumber;
                        //    //orderedTransactions[i].BlockId;
                        //    //orderedTransactions[i].Fees;
                        //    //orderedTransactions[i].IsIrreversible;
                        //    //orderedTransactions[i].SpentCoins;
                        //    //orderedTransactions[i].TransactionId;
                        //    //orderedTransactions[i].ReceivedCoins;

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
                }
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

        private static bool AssertEqual<T>(T obj1, T obj2) where T : IComparable<T>
        {
            return obj1?.CompareTo(obj2) == 0;
        }
    }

    public interface IBlockchainVerifierAdapter
    {
        Task<(IEnumerable<TransferCoinsTransactionExecutedEvent>coinTransfers, IEnumerable<TransactionFailedEvent>
                failedTransfers)>
            GetTransactionsForBlockAsync(BigInteger blockNumber);

        Task<BlockHeader> GetBlockAsync(BigInteger blockNumber);
    }

    public class BlockchainVerifierAdapterFactory
    {
        public BlockchainVerifierAdapterFactory()
        { }

        public IBlockchainVerifierAdapter GetAdapter(string blockchainType, params string[] args)
        {
            switch (blockchainType)
            {
                case "Bitcoin":
                    return new BlockchainVerifierAdapter(args[0], Network.GetNetwork(args[1]));
                default:
                    return null;
            }
        }
    }

    public class BlockchainVerifierAdapter : IBlockchainVerifierAdapter
    {
        private readonly QBitNinjaClient _ninjaClient;

        public BlockchainVerifierAdapter(string ninjaUrl, Network network)
        {
            if (string.IsNullOrEmpty(ninjaUrl))
                throw new ArgumentException("Should not be empty", nameof(ninjaUrl));

            _ninjaClient = new QBitNinjaClient(ninjaUrl, network);
        }

        public async Task<(IEnumerable<TransferCoinsTransactionExecutedEvent> coinTransfers, IEnumerable<TransactionFailedEvent> failedTransfers)> GetTransactionsForBlockAsync(BigInteger blockNumber)
        {
            var listTransfered = new List<TransferCoinsTransactionExecutedEvent>();


            return (listTransfered, new TransactionFailedEvent[] {});
        }

        public async Task<BlockHeader> GetBlockAsync(BigInteger blockNumber)
        {
            var block = await _ninjaClient.GetBlock(BlockFeature.Parse(blockNumber.ToString()), true, false);
            var blockHash = block.Block.Header.GetHash().ToString();
            var version = block.Block.Header.Version;
            var size = 0;
            var transactionCount = block.Block.Transactions.Count;
            var previousBlockId = block.Block.Header.HashPrevBlock.ToString();

            return new BlockHeader(blockHash,
                version,
                "Bitcoin",
                (long)blockNumber,
                block.Block.Header.BlockTime.DateTime,
                size,
                transactionCount,
                previousBlockId,
                BlockState.Executed
                );
        }
    }
}
