using Common;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.Ripple.Client;
using Lykke.Bil2.Ripple.Client.Api.Ledger;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Numerics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Lykke.Common.Log;
using Lykke.Logs.Loggers.LykkeConsole;
using BlockHeader = Lykke.Job.Bil2Indexer.Domain.BlockHeader;

namespace Lykke.Job.Bil2Indexer.VerifyingTool.BlockchainAdapters.Ripple
{
    public class RippleBlockchainVerifierAdapter : IBlockchainVerifierAdapter
    {
        private readonly IRippleApi _rippleApi;

        public RippleBlockchainVerifierAdapter(string rippleUrl, string username = null, string password = null)
        {
            if (string.IsNullOrEmpty(rippleUrl))
                throw new ArgumentException("Should not be empty", nameof(rippleUrl));

            IServiceCollection serviceCollection = new ServiceCollection();
            var logFactory = Logs.EmptyLogFactory.Instance;
            logFactory.AddConsole();
            serviceCollection.AddSingleton<ILogFactory>(logFactory);
            serviceCollection.AddRippleClient(rippleUrl, username, password);
            _rippleApi = serviceCollection.BuildServiceProvider().GetRequiredService<IRippleApi>();
        }

        public async Task<IReadOnlyCollection<Transaction>> GetBlockTransactionsAsync(BigInteger blockNumber)
        {
            var transactions = new List<Transaction>();
            var block = await _rippleApi.Post(new BinaryLedgerWithTransactionsRequest((uint)blockNumber));
            var result = block.Result;

            foreach (var transaction in result.Ledger.Transactions)
            {
                var tx = transaction.Parse();
                var txNumber = (int)(tx.Metadata.TransactionIndex + 1);
                var txFee = new[]
                {
                    new Fee
                    (
                        new Asset("XRP"),
                        new UMoney(BigInteger.Parse(tx.Fee), 6)
                    )
                };

                if (tx.Metadata.TransactionResult == "tesSUCCESS")
                {
                    var transferAmountTransaction = new TransferAmountExecutedTransaction
                    (
                        txNumber,
                        tx.Hash,
                        tx.Metadata
                            .GetBalanceChanges()
                            .SelectMany(pair => pair.Value.Select(amount => (address: pair.Key, amount: amount)))
                            .Select(pair => new BalanceChange
                            (
                                tx.Metadata.TransactionIndex.ToString(),
                                new Asset(pair.amount.Currency, pair.amount.Counterparty),
                                Lykke.Numerics.Money.Parse(pair.amount.Value),
                                pair.address,
                                pair.address == tx.Destination ? tx.DestinationTag?.ToString("D") : null,
                                pair.address == tx.Destination && tx.DestinationTag != null
                                    ? AddressTagType.Number
                                    : (AddressTagType?)null,
                                pair.address == tx.Account ? tx.Sequence : (long?)null
                            ))
                            .ToArray(),
                        txFee,
                        result.Validated ?? false
                    );

                    var domainTransaction = new Transaction
                    (
                        "Ripple",
                        result.LedgerHash,
                        transferAmountTransaction
                    );

                    transactions.Add(domainTransaction);
                }
                else
                {
                    var failedTransaction = new FailedTransaction
                    (
                        txNumber,
                        tx.Hash,
                        tx.Metadata.TransactionResult == "tecUNFUNDED" ||
                        tx.Metadata.TransactionResult == "tecUNFUNDED_PAYMENT"
                            ? TransactionBroadcastingError.NotEnoughBalance
                            : TransactionBroadcastingError.TransientFailure,
                        tx.Metadata.TransactionResult,
                        txFee
                    );

                    var domainTransaction = new Transaction
                    (
                        "Ripple",
                        result.LedgerHash,
                        failedTransaction
                    );

                    transactions.Add(domainTransaction);
                }
            }

            return transactions;
        }

        public async Task<BlockHeader> GetBlockAsync(BigInteger blockNumber)
        {
            var block = await _rippleApi.Post(new BinaryLedgerWithTransactionsRequest((uint)blockNumber));
            var result = block.Result;
            var ledger = result.Ledger.Parse();
            var blockHash = result.LedgerHash;
            var version = 1;
            var size = block.Result.Ledger.LedgerData.GetHexStringToBytes().Length;
            var transactionCount = block.Result.Ledger.Transactions?.Length ?? 0;
            var previousBlockId = ledger.ParentHash;

            return new BlockHeader(blockHash,
                version,
                "Ripple",
                (long)blockNumber,
                ledger.CloseTime.FromRippleEpoch(),
                size,
                transactionCount,
                previousBlockId,
                BlockState.Executed
            );
        }
    }
}
