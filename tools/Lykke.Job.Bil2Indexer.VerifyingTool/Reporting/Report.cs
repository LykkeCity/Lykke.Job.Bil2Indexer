using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.SharedDomain;
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
                    if (_transferModel == BlockchainTransferModel.Coins)
                    {
                        string continuation = null;
                        var transfers = new List<TransferCoinsTransactionExecutedEvent>(indexedBlock.TransactionsCount);
                        var failedTransfers = new List<TransactionFailedEvent>(indexedBlock.TransactionsCount);

                        do
                        {
                            var paginationResponse = await
                                _transactionsRepository.GetAllOfBlockAsync(
                                    _blockchainType,
                                    indexedBlock.Id,
                                    _limit,
                                    continuation);

                            continuation = paginationResponse.Continuation;

                            transfers.AddRange(paginationResponse.Items.Where(x => x.IsTransferCoins).Select(x => x.AsTransferCoins()));
                            failedTransfers.AddRange(paginationResponse.Items.Where(x => x.IsFailed).Select(x => x.AsFailed()));
                        } while (!string.IsNullOrEmpty(continuation));

                        var orderedTransactions = transfers.OrderBy(x => x.TransactionNumber).ToArray();
                        var (realCoinTransfers, realFailedEvents) =
                            await _adapter.GetCoinTransactionsForBlockAsync(currentBlockNumber);
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
                            _reportingContext.StartScope();

                            var orderedTransactionIndexed = orderedTransactions[j];
                            var orderedTransactionReal = orderedRealCoinTransfers[j];

                            AssertCoinTransfers(orderedTransactionIndexed, orderedTransactionReal);

                            var spentCoinsReal = orderedTransactionReal.SpentCoins.ToArray();
                            var spentCoinsIndexed = orderedTransactionIndexed.SpentCoins.ToArray();

                            AssertEqual(
                                spentCoinsReal.Length,
                                spentCoinsReal.Length,
                                "SpentCoinsReal.Length");

                            AssertSpentCoins(spentCoinsReal, spentCoinsIndexed);

                            var receivedCoinsReal = orderedTransactionReal.ReceivedCoins.ToArray();
                            var receivedCoinsIndexed = orderedTransactionIndexed.ReceivedCoins.ToArray();

                            AssertEqual(
                                receivedCoinsReal.Length,
                                receivedCoinsReal.Length,
                                "ReceivedCoinsReal.Length");

                            AssertReceivedCoins(receivedCoinsReal, receivedCoinsIndexed);

                            var feesReal = orderedTransactionReal.Fees?.ToArray();
                            var feesIndexed = orderedTransactionIndexed.Fees?.ToArray();

                            AssertEqual(
                                feesReal?.Length ?? 0,
                                feesIndexed?.Length ?? 0,
                                "Fees.Length");

                            AssertFees(feesReal, feesIndexed);

                            _reportingContext.EndScope();
                        }

                        _reportingContext.EndScope();
                    }
                    else if (_transferModel == BlockchainTransferModel.Amount)
                    {
                        string continuation = null;
                        var transfers = new List<TransferAmountTransactionExecutedEvent>(indexedBlock.TransactionsCount);
                        var failedTransfers = new List<TransactionFailedEvent>(indexedBlock.TransactionsCount);

                        do
                        {
                            var paginationResponse = await
                                _transactionsRepository.GetAllOfBlockAsync(
                                    _blockchainType,
                                    indexedBlock.Id,
                                    _limit,
                                    continuation);

                            continuation = paginationResponse.Continuation;

                            transfers.AddRange(paginationResponse.Items.Where(x => x.IsTransferAmount).Select(x => x.AsTransferAmount()));
                            failedTransfers.AddRange(paginationResponse.Items.Where(x => x.IsFailed).Select(x => x.AsFailed()));
                        } while (!string.IsNullOrEmpty(continuation));

                        var orderedTransactions = transfers.OrderBy(x => x.TransactionNumber).ToArray();
                        var (realAmountTransfers, realFailedEvents) =
                            await _adapter.GetAmountTransactionsForBlockAsync(currentBlockNumber);
                        var orderedRealAmountTransfers = realAmountTransfers.OrderBy(x => x.TransactionNumber).ToArray();
                        var orderedRealFailedEvents = realFailedEvents.OrderBy(x => x.TransactionNumber).ToArray();

                        AssertEqual(
                            orderedTransactions.Length,
                            orderedRealAmountTransfers.Count(),
                            nameof(realAmountTransfers));
                        AssertEqual(failedTransfers.Count,
                            orderedRealFailedEvents.Count(),
                            nameof(realFailedEvents));

                        _reportingContext.StartListScope("amountTransfers");

                        for (int j = 0; j < orderedRealAmountTransfers.Length; j++)
                        {
                            _reportingContext.StartScope();

                            var orderedTransactionIndexed = orderedTransactions[j];
                            var orderedTransactionReal = orderedRealAmountTransfers[j];

                            AssertAmountTransfers(orderedTransactionIndexed, orderedTransactionReal);

                            var balanceChangesReal = orderedTransactionReal.BalanceChanges.ToArray();
                            var balanceChangesIndexed = orderedTransactionIndexed.BalanceChanges.ToArray();

                            AssertEqual(
                                balanceChangesReal.Length,
                                balanceChangesIndexed.Length,
                                "BalanceChanges.Length");

                            AssertBalanceChanges(balanceChangesReal, balanceChangesIndexed);

                            var feesReal = orderedTransactionReal.Fees?.ToArray();
                            var feesIndexed = orderedTransactionIndexed.Fees?.ToArray();

                            AssertEqual(
                                feesReal?.Length ?? 0,
                                feesIndexed?.Length ?? 0,
                                "Fees.Length");

                            AssertFees(feesReal, feesIndexed);

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

        private void AssertFees(Fee[] feesReal, Fee[] feesIndexed)
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

            _reportingContext.EndScope();
        }

        private void AssertReceivedCoins(ReceivedCoin[] receivedCoinsReal, ReceivedCoin[] receivedCoinsIndexed)
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
                AssertEqual((int?)receivedCoinIndexed.AddressTagType ?? 0,
                    (int?)receivedCoinReal.AddressTagType ?? 0, nameof(receivedCoinReal.AddressTagType));
                AssertEqual(receivedCoinIndexed.Value, receivedCoinReal.Value, nameof(receivedCoinReal.Value));
                AssertEqual(receivedCoinIndexed.Asset, receivedCoinReal.Asset, nameof(receivedCoinReal.Asset));

                _reportingContext.EndScope();
            }

            _reportingContext.EndScope();
        }

        private void AssertBalanceChanges(BalanceChange[] balanceChangeReal, BalanceChange[] balanceChangeIndexed)
        {
            _reportingContext.StartListScope("balanceChange");

            for (int balanceChangeIndex = 0; balanceChangeIndex < balanceChangeReal.Length; balanceChangeIndex++)
            {
                _reportingContext.StartScope();

                var spentCoinIndexed = balanceChangeIndexed[balanceChangeIndex];
                var spentCoinReal = balanceChangeReal[balanceChangeIndex];

                AssertEqual(spentCoinIndexed.Value,
                    spentCoinReal.Value,
                    nameof(spentCoinReal.Value));

                AssertEqual(spentCoinIndexed.Address,
                    spentCoinReal.Address,
                    nameof(spentCoinReal.Address));

                AssertEqual(spentCoinIndexed.Tag,
                    spentCoinReal.Tag,
                    nameof(spentCoinReal.Tag));

                AssertEqual((int)(spentCoinIndexed.TagType ?? AddressTagType.Number),
                    (int)(spentCoinReal.TagType ?? AddressTagType.Number),
                    nameof(spentCoinReal.TagType));

                AssertEqual(spentCoinIndexed.Asset,
                    spentCoinReal.Asset,
                    nameof(spentCoinReal.Asset));

                _reportingContext.EndScope();
            }

            _reportingContext.EndScope();
        }

        private void AssertSpentCoins(CoinId[] spentCoinsReal, CoinId[] spentCoinsIndexed)
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

        private void AssertAmountTransfers(TransferAmountTransactionExecutedEvent orderedTransactionIndexed,
            TransferAmountTransactionExecutedEvent orderedTransactionReal)
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

        private void AssertCoinTransfers(TransferCoinsTransactionExecutedEvent orderedTransactionIndexed,
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

        private void AssertBlockHeaders(BlockHeader indexedBlock, BlockHeader realBlock)
        {
            _reportingContext.StartScope("blocksHeader");

            if (indexedBlock != null)
            {
                AssertEqual(indexedBlock.BlockchainType, realBlock.BlockchainType, nameof(realBlock.BlockchainType));
                AssertEqual(indexedBlock.Id, realBlock.Id, nameof(realBlock.Id));
                AssertEqual(indexedBlock.Number, realBlock.Number, nameof(realBlock.Number));
                AssertEqual(indexedBlock.PreviousBlockId, realBlock.PreviousBlockId, nameof(realBlock.PreviousBlockId));
                AssertEqual(indexedBlock.TransactionsCount, realBlock.TransactionsCount,
                    nameof(realBlock.TransactionsCount));
                AssertEqual(indexedBlock.MinedAt, realBlock.MinedAt, nameof(realBlock.MinedAt));
                AssertEqual(indexedBlock.Size, realBlock.Size, nameof(realBlock.Size));
            }
            else
            {
                AssertEqual(null, realBlock.Id, nameof(realBlock.Id));
            }

            _reportingContext.EndScope();
        }

        private IBlockchainVerifierAdapter InitAdapter(string blockchainType, string[] args)
        {
            BlockchainVerifierAdapterFactory factory = new BlockchainVerifierAdapterFactory();
            return factory.GetAdapter(blockchainType, args);
        }

        private void AssertEqual<T>(T indexedField, T realField, string fieldName = null) where T : IComparable<T>
        {
            if (indexedField == null && realField == null)
                return;

            if (indexedField?.CompareTo(realField) != 0)
            {
                _reportingContext.SetError(fieldName, indexedField, realField);
            }
        }
    }
}
