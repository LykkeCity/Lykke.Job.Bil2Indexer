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

        public async Task<(IEnumerable<TransferCoinsTransactionExecutedEvent> coinTransfers, IEnumerable<TransactionFailedEvent> failedTransfers)>
            GetCoinTransactionsForBlockAsync(BigInteger blockNumber)
        {
            throw new NotImplementedException();
        }

        public async Task<(IEnumerable<TransferAmountTransactionExecutedEvent> amountTransfers, IEnumerable<TransactionFailedEvent> failedTransfers)>
            GetAmountTransactionsForBlockAsync(BigInteger blockNumber)
        {
            List<TransferAmountTransactionExecutedEvent> transfers = new List<TransferAmountTransactionExecutedEvent>();
            List<TransactionFailedEvent> failedTransfers = new List<TransactionFailedEvent>();
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
                    var item = new TransferAmountTransactionExecutedEvent
                    (
                        result.LedgerHash,
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

                    transfers.Add(item);
                }
                else
                {
                    var item = new TransactionFailedEvent
                    (
                        result.LedgerHash,
                        txNumber,
                        tx.Hash,
                        tx.Metadata.TransactionResult == "tecUNFUNDED" ||
                        tx.Metadata.TransactionResult == "tecUNFUNDED_PAYMENT"
                            ? TransactionBroadcastingError.NotEnoughBalance
                            : TransactionBroadcastingError.TransientFailure,
                        tx.Metadata.TransactionResult,
                        txFee
                    );

                    failedTransfers.Add(item);
                }
            }

            return (transfers, failedTransfers);
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
