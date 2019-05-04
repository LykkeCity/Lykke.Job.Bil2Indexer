using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations;
using Lykke.Job.Bil2Indexer.VerifyingTool.BlockchainAdapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.VerifyingTool.Reporting
{
    public class Report
    {
        private static readonly int _limit = 100;
        private ReportingContext _reportingContext;
        private readonly SemaphoreSlim _syncRoot = new SemaphoreSlim(1);
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly string _blockchainType;
        private readonly IBlockchainVerifierAdapter _adapter;
        private readonly BlockchainTransferModel _transferModel;
        private readonly ITransactionsRepository _transactionsRepository;

        public Report(IBlockHeadersRepository blockHeadersRepository, 
            string blockchainType, 
            IBlockchainVerifierAdapter adapter,
            BlockchainTransferModel transferModel,
            ITransactionsRepository transactionsRepository)
        {
            _blockHeadersRepository = blockHeadersRepository;
            _blockchainType = blockchainType;
            _adapter = adapter;
            _transferModel = transferModel;
            _transactionsRepository = transactionsRepository;
        }

        public async Task CreateReportAsync(BigInteger fromBlock,
            BigInteger toBlock)
        {
            await _syncRoot.WaitAsync();

            using (_reportingContext = new ReportingContext("report_" + _blockchainType))
            {
                await MakeReportAsync(fromBlock, toBlock);
            }

            _syncRoot.Release();
        }

        private async Task MakeReportAsync(BigInteger fromBlock, 
            BigInteger toBlock)
        {
            _reportingContext.StartListScope();

            for (var currentBlockNumber = fromBlock; currentBlockNumber <= toBlock; currentBlockNumber++)
            {
                _reportingContext.StartScope();

                var indexedBlock = await _blockHeadersRepository.GetOrDefaultAsync(_blockchainType, (long)currentBlockNumber);
                var realBlock = await _adapter.GetBlockAsync(currentBlockNumber);

                #region Check Block Indexation 

                _reportingContext.CurrentReportObject.currentBlockNumber = currentBlockNumber.ToString();
                AssertBlockHeaders(indexedBlock, realBlock);

                #endregion

                #region Check Transaction Indexation

                if (indexedBlock != null)
                {
                    string continuation = null;
                    var transactions = new List<Transaction>(indexedBlock.TransactionsCount);

                    do
                    {
                        var paginationResponse = await
                            _transactionsRepository.GetAllOfBlockAsync(
                                _blockchainType,
                                indexedBlock.Id,
                                _limit,
                                continuation);

                        continuation = paginationResponse.Continuation;

                        transactions.AddRange(paginationResponse.Items);
                    } while (!string.IsNullOrEmpty(continuation));

                    if (_transferModel == BlockchainTransferModel.Coins)
                    {
                        var orderedTransactions = transactions
                            .OrderBy(x => x.AsTransferCoinsOrDefault()?.TransactionNumber ?? x.AsFailed().TransactionNumber)
                            .ToArray();
                        var realTransactions = await _adapter.GetBlockTransactionsAsync(currentBlockNumber);
                        var orderedRealTransactions = realTransactions
                            .OrderBy(x => x.AsTransferCoinsOrDefault()?.TransactionNumber ?? x.AsFailed().TransactionNumber)
                            .ToArray();

                        AssertEqual(
                            orderedTransactions.Length,
                            orderedRealTransactions.Length,
                            nameof(orderedRealTransactions));

                        _reportingContext.StartListScope("coinTransfers");

                        for (var j = 0; j < orderedRealTransactions.Length; j++)
                        {
                            _reportingContext.StartScope();

                            var indexedTransaction = orderedTransactions[j];
                            var realTransaction = orderedRealTransactions[j];
                            var indexedTransferCoinsTransaction = indexedTransaction.AsTransferCoinsOrDefault();
                            var realTransferCoinsTransaction = realTransaction.AsTransferCoinsOrDefault();

                            AssertCommon(indexedTransaction, realTransaction);
                            AssertCoinTransfers(indexedTransferCoinsTransaction, realTransferCoinsTransaction);                           
                            AssertFees(indexedTransaction, realTransaction);

                            _reportingContext.EndScope();
                        }

                        _reportingContext.EndScope();
                    }
                    else if (_transferModel == BlockchainTransferModel.Amount)
                    {
                        var orderedTransactions = transactions
                            .OrderBy(x => x.AsTransferAmountOrDefault()?.TransactionNumber ?? x.AsFailed().TransactionNumber)
                            .ToArray();
                        var realTransactions = await _adapter.GetBlockTransactionsAsync(currentBlockNumber);
                        var orderedRealTransactions = realTransactions
                            .OrderBy(x => x.AsTransferAmountOrDefault()?.TransactionNumber ?? x.AsFailed().TransactionNumber)
                            .ToArray();

                        AssertEqual(
                            orderedTransactions.Length,
                            orderedRealTransactions.Length,
                            nameof(orderedRealTransactions));

                        _reportingContext.StartListScope("amountTransfers");

                        for (var j = 0; j < orderedRealTransactions.Length; j++)
                        {
                            _reportingContext.StartScope();

                            var indexedTransaction = orderedTransactions[j];
                            var realTransaction = orderedRealTransactions[j];
                            var indexedTransferAmountTransaction = indexedTransaction.AsTransferAmountOrDefault();
                            var realTransferAmountTransaction = realTransaction.AsTransferAmountOrDefault();

                            AssertCommon(indexedTransaction, realTransaction);
                            AssertAmountTransfers(indexedTransferAmountTransaction, realTransferAmountTransaction);
                            AssertFees(indexedTransaction, realTransaction);

                            _reportingContext.EndScope();
                        }

                        _reportingContext.EndScope();
                    }
                    else
                    {
                        throw new NotSupportedException($"It is not stated whether {_blockchainType} " +
                                                        $"supports {BlockchainTransferModel.Coins} or {BlockchainTransferModel.Amount}");
                    }
                }

                #endregion

                _reportingContext.EndScope();
                await _reportingContext.FlushAsync();
            }
        }

        private void AssertFees(Transaction indexedTransaction, Transaction realTransaction)
        {
            _reportingContext.StartListScope("fees");

            var indexedFees = indexedTransaction.AsTransferCoinsOrDefault().Fees ??
                              indexedTransaction.AsTransferAmountOrDefault().Fees ??
                              indexedTransaction.AsFailed().Fees;
            var realFees = realTransaction.AsTransferCoinsOrDefault().Fees ??
                          realTransaction.AsTransferAmountOrDefault().Fees ??
                          realTransaction.AsFailed().Fees;

            AssertEqual(indexedFees != null, realFees != null, "Fees.Existence");
            
            if (indexedFees != null && realFees != null)
            {
                AssertEqual(indexedFees.Count, realFees.Count, "Fees.Count");

                var indexedFeesArray = indexedFees.ToArray();
                var realFeesArray = realFees.ToArray();

                for (var j = 0; j < realFeesArray.Length; j++)
                {
                    _reportingContext.StartScope();

                    var indexedFee = indexedFeesArray[j];
                    var realFee = realFeesArray[j];

                    AssertEqual(indexedFee.Amount, realFee.Amount, nameof(realFee.Amount));
                    AssertEqual(indexedFee.Asset, realFee.Asset, nameof(realFee.Asset));

                    _reportingContext.EndScope();
                }
            }

            _reportingContext.EndScope();
        }

        private void AssertReceivedCoins(TransferCoinsExecutedTransaction indexedTransaction, TransferCoinsExecutedTransaction realTransaction)
        {
            _reportingContext.StartListScope("receivedCoins");

            AssertEqual(indexedTransaction.ReceivedCoins.Count, realTransaction.ReceivedCoins.Count, "ReceivedCoins.Count");

            var indexedCoins = indexedTransaction.ReceivedCoins.ToArray();
            var realCoins = realTransaction.ReceivedCoins.ToArray();

            for (var j = 0; j < realCoins.Length; j++)
            {
                _reportingContext.StartScope();

                var indexedCoin = indexedCoins[j];
                var realCoin = realCoins[j];

                AssertEqual(indexedCoin.CoinNumber, realCoin.CoinNumber, nameof(realCoin.CoinNumber));
                AssertEqual(indexedCoin.Address, realCoin.Address, nameof(realCoin.Address));
                AssertEqual(indexedCoin.AddressNonce, realCoin.AddressNonce, nameof(realCoin.AddressNonce));
                AssertEqual(indexedCoin.AddressTag, realCoin.AddressTag, nameof(realCoin.AddressTag));
                AssertEqual(indexedCoin.AddressTagType, realCoin.AddressTagType, nameof(realCoin.AddressTagType));
                AssertEqual(indexedCoin.Value, realCoin.Value, nameof(realCoin.Value));
                AssertEqual(indexedCoin.Asset, realCoin.Asset, nameof(realCoin.Asset));

                _reportingContext.EndScope();
            }

            _reportingContext.EndScope();
        }

        private void AssertBalanceChanges(TransferAmountExecutedTransaction indexedTransaction, TransferAmountExecutedTransaction realTransaction)
        {
            _reportingContext.StartListScope("balanceChange");

            AssertEqual(indexedTransaction.BalanceChanges.Count, realTransaction.BalanceChanges.Count, "BalanceChanges.Count");

            var indexedBalanceChanges = indexedTransaction.BalanceChanges.ToArray();
            var realBalanceChanges = realTransaction.BalanceChanges.ToArray();

            for (var j = 0; j < realBalanceChanges.Length; j++)
            {
                _reportingContext.StartScope();

                var indexedBalanceChange = indexedBalanceChanges[j];
                var realBalanceChange = realBalanceChanges[j];

                AssertEqual(indexedBalanceChange.Value, realBalanceChange.Value, nameof(realBalanceChange.Value));
                AssertEqual(indexedBalanceChange.Address, realBalanceChange.Address, nameof(realBalanceChange.Address));
                AssertEqual(indexedBalanceChange.Tag, realBalanceChange.Tag, nameof(realBalanceChange.Tag));
                AssertEqual(indexedBalanceChange.TagType, realBalanceChange.TagType, nameof(realBalanceChange.TagType));
                AssertEqual(indexedBalanceChange.Asset, realBalanceChange.Asset, nameof(realBalanceChange.Asset));

                _reportingContext.EndScope();
            }

            _reportingContext.EndScope();
        }

        private void AssertSpentCoins(TransferCoinsExecutedTransaction indexedTransaction, TransferCoinsExecutedTransaction realTransaction)
        {
            _reportingContext.StartListScope("spentCoin");

            AssertEqual(indexedTransaction.SpentCoins.Count, realTransaction.SpentCoins.Count, "SpentCoins.Count");

            var indexedCoins = indexedTransaction.SpentCoins.ToArray();
            var realCoins = realTransaction.SpentCoins.ToArray();

            for (var j = 0; j < realCoins.Length; j++)
            {
                _reportingContext.StartScope();

                var indexedCoin = indexedCoins[j];
                var realCoin = realCoins[j];

                AssertEqual(indexedCoin.CoinNumber, realCoin.CoinNumber, nameof(realCoin.CoinNumber));
                AssertEqual(indexedCoin.TransactionId, realCoin.TransactionId, nameof(realCoin.TransactionId));

                _reportingContext.EndScope();
            }

            _reportingContext.EndScope();
        }

        private void AssertCommon(Transaction indexedTransaction, Transaction realTransaction)
        {
            AssertEqual(indexedTransaction.BlockId, realTransaction.BlockId, nameof(realTransaction.BlockId));
            AssertEqual(indexedTransaction.Type, realTransaction.Type, nameof(realTransaction.Type));
        }

        private void AssertAmountTransfers(TransferAmountExecutedTransaction indexedTransaction, TransferAmountExecutedTransaction realTransaction)
        {
            if (indexedTransaction == null || realTransaction == null)
            {
                return;
            }

            AssertEqual(indexedTransaction.TransactionNumber, realTransaction.TransactionNumber, nameof(realTransaction.TransactionNumber));
            AssertEqual(indexedTransaction.TransactionId, realTransaction.TransactionId, nameof(realTransaction.TransactionId));
            AssertEqual(indexedTransaction.IsIrreversible, realTransaction.IsIrreversible, nameof(realTransaction.IsIrreversible));

            AssertBalanceChanges(indexedTransaction, realTransaction);
        }

        private void AssertCoinTransfers(TransferCoinsExecutedTransaction indexedTransaction, TransferCoinsExecutedTransaction realTransaction)
        {
            if (indexedTransaction == null || realTransaction == null)
            {
                return;
            }

            AssertEqual(indexedTransaction.TransactionNumber, realTransaction.TransactionNumber, nameof(realTransaction.TransactionNumber));
            AssertEqual(indexedTransaction.TransactionId, realTransaction.TransactionId, nameof(realTransaction.TransactionId));
            AssertEqual(indexedTransaction.IsIrreversible, realTransaction.IsIrreversible, nameof(realTransaction.IsIrreversible));

            AssertReceivedCoins(indexedTransaction, realTransaction);
            AssertSpentCoins(indexedTransaction, realTransaction);
        }

        private void AssertBlockHeaders(BlockHeader indexedBlock, BlockHeader realBlock)
        {
            _reportingContext.StartScope("blocksHeader");

            if (indexedBlock != null)
            {
                AssertEqual(indexedBlock.BlockchainType, realBlock.BlockchainType, nameof(realBlock.BlockchainType));
                AssertEqual(indexedBlock.Id, realBlock.Id, nameof(realBlock.Id));
                AssertEqual(indexedBlock.Number, realBlock.Number, nameof(realBlock.Number));
                AssertEqual(indexedBlock.PreviousBlockId, realBlock.PreviousBlockId, nameof(realBlock.PreviousBlockId));
                AssertEqual(indexedBlock.TransactionsCount, realBlock.TransactionsCount, nameof(realBlock.TransactionsCount));
                AssertEqual(indexedBlock.MinedAt, realBlock.MinedAt, nameof(realBlock.MinedAt));
                AssertEqual(indexedBlock.Size, realBlock.Size, nameof(realBlock.Size));
            }
            else
            {
                AssertEqual(null, realBlock.Id, nameof(realBlock.Id));
            }

            _reportingContext.EndScope();
        }

        private void AssertEqual<T>(T indexedField, T realField, string fieldName = null)
        {
            if (!Equals(indexedField, realField))
            {
                _reportingContext.SetError(fieldName, indexedField, realField);
            }
        }
    }
}
