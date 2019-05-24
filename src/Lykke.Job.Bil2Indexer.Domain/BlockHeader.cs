using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Contract;
using Lykke.Job.Bil2Indexer.Domain.Infrastructure;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Numerics;
using Lykke.Numerics.Linq;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class BlockHeader
    {
        public BlockId Id { get; }
        public long Version { get; }
        public string BlockchainType { get; }
        public long Number { get; }
        public DateTime MinedAt { get; }
        public int Size { get; }
        public int TransactionsCount { get; }
        public BlockId PreviousBlockId { get; }
        public BlockState State { get; private set; }

        public bool IsNotAssembledYet => State == BlockState.Assembling;

        public bool IsNotExecutedYet =>
            State == BlockState.Assembling ||
            State == BlockState.Assembled;

        public bool IsAssembled => State == BlockState.Assembled;

        public bool IsAlreadyAssembled =>
            State == BlockState.Assembled ||
            State == BlockState.Executed ||
            State == BlockState.PartiallyExecuted;

        public bool IsExecuted => State == BlockState.Executed;

        public bool IsPartiallyExecuted => State == BlockState.PartiallyExecuted;

        public bool CanBeExecuted => State == BlockState.Assembled || State == BlockState.PartiallyExecuted;

        public bool ExecutionCanBeReverted => State == BlockState.Executed || State == BlockState.PartiallyExecuted;
        

        public BlockHeader(
            BlockId id, 
            long version,
            string blockchainType, 
            long number, 
            DateTime minedAt, 
            int size,
            int transactionsCount, 
            BlockId previousBlockId,
            BlockState state)
        {
            Id = id;
            Version = version;
            BlockchainType = blockchainType;
            Number = number;
            MinedAt = minedAt;
            Size = size;
            TransactionsCount = transactionsCount;
            PreviousBlockId = previousBlockId;
            State = state;
        }

        public static BlockHeader StartAssembling(
            BlockId id, 
            string blockchainType, 
            long number, 
            DateTime minedAt, 
            int size,
            int transactionsCount, 
            BlockId previousBlockId)
        {
            return new BlockHeader
            (
                id,
                0,
                blockchainType,
                number,
                minedAt,
                size,
                transactionsCount,
                previousBlockId,
                BlockState.Assembling
            );
        }

        public async Task<bool> TryToAssembleAsync(ITransactionsRepository transactionsRepository)
        {
            if (State != BlockState.Assembling)
            {
                throw new InvalidOperationException($"Expected state: {BlockState.Assembling}, actual: {State}");
            }

            var storedTransactionsCount = await transactionsRepository.CountInBlockAsync(BlockchainType, Id);

            if (TransactionsCount == storedTransactionsCount)
            {
                State = BlockState.Assembled;

                return true;
            }
            
            return false;
        }

        public async Task ExecuteAsync(
            ITransactionsRepository transactionsRepository,
            ICoinsRepository coinsRepository,
            IBalanceActionsRepository balanceActionsRepository,
            IFeeEnvelopesRepository feeEnvelopesRepository,
            bool haveToExecuteEntireBlock)
        {
            if (!CanBeExecuted)
            {
                throw new InvalidOperationException($"Block can be executed only in states: {BlockState.Assembled} or {BlockState.PartiallyExecuted}, actual: {State}");
            }

            PaginatedItems<Transaction> transactions = null;
            var spendCoinsTask = default(Task);
            var notExecutedTransactions = new ConcurrentBag<TransferCoinsExecutedTransaction>();

            do
            {
                var coinsToSpend = new ConcurrentBag<CoinId>();
                
                transactions = await transactionsRepository.GetAllOfBlockAsync
                (
                    BlockchainType,
                    Id, 
                    500,
                    transactions?.Continuation
                );

                var isAllTransactionsExecuted = await transactions.Items
                    .Where(x => x.IsTransferCoins)
                    .Select(x => x.AsTransferCoins())
                    .ForEachAsync
                    (
                        degreeOfParallelism: 8,
                        body: async transaction =>
                        {
                            
                            var isExecuted = await TryExecuteTransactionAsync
                            (
                                coinsRepository,
                                balanceActionsRepository,
                                feeEnvelopesRepository,
                                transaction,
                                coinsToSpend
                            );

                            if (!isExecuted)
                            {
                                notExecutedTransactions.Add(transaction);

                                return haveToExecuteEntireBlock;
                            }

                            return true;
                        }
                    );

                if (spendCoinsTask != default(Task))
                {
                    await spendCoinsTask;
                }

                if (!isAllTransactionsExecuted)
                {
                    State = BlockState.PartiallyExecuted;
                    return;
                }

                spendCoinsTask = coinsRepository.SpendAsync(BlockchainType, coinsToSpend);

            } while (transactions.Continuation != null);

            if (haveToExecuteEntireBlock)
            {
                var currentNotExecutedTransactions = notExecutedTransactions;

                while (currentNotExecutedTransactions.Any())
                {
                    var coinsToSpend = new ConcurrentBag<CoinId>();
                    var remainderOfNotExecutedTransactions = new ConcurrentBag<TransferCoinsExecutedTransaction>();

                    await currentNotExecutedTransactions
                        .ForEachAsync
                        (
                            degreeOfParallelism: 8,
                            body: async transaction =>
                            {

                                var isExecuted = await TryExecuteTransactionAsync
                                (
                                    coinsRepository,
                                    balanceActionsRepository,
                                    feeEnvelopesRepository,
                                    transaction,
                                    coinsToSpend
                                );

                                if (!isExecuted)
                                {
                                    remainderOfNotExecutedTransactions.Add(transaction);
                                }
                            }
                        );

                    if (spendCoinsTask != default(Task))
                    {
                        await spendCoinsTask;
                    }

                    if (remainderOfNotExecutedTransactions.Count == currentNotExecutedTransactions.Count)
                    {
                        throw new InvalidOperationException($"Can't execute entire block. Coins to spend by {remainderOfNotExecutedTransactions.Count} transactions are missed");
                    }

                    spendCoinsTask = coinsRepository.SpendAsync(BlockchainType, coinsToSpend);

                    currentNotExecutedTransactions = remainderOfNotExecutedTransactions;
                }
            }

            await spendCoinsTask;

            State = BlockState.Executed;
        }

        private async Task<bool> TryExecuteTransactionAsync(
            ICoinsRepository coinsRepository,
            IBalanceActionsRepository balanceActionsRepository,
            IFeeEnvelopesRepository feeEnvelopesRepository,
            TransferCoinsExecutedTransaction transaction, 
            ConcurrentBag<CoinId> coinsToSpend)
        {
            // TODO: Get Batch for all transactions in the page
            var coinsToSpendByTransaction = await coinsRepository.GetSomeOfAsync(BlockchainType, transaction.SpentCoins);
            var coinsMissedInTransaction = transaction.SpentCoins.Except(coinsToSpendByTransaction.Select(x => x.Id)).ToArray();

            if (coinsMissedInTransaction.Any())
            {
                return false;
            }

            await ExecuteTransactionAsync
            (
                coinsToSpendByTransaction,
                balanceActionsRepository,
                feeEnvelopesRepository,
                transaction
            );

            foreach (var coin in coinsToSpendByTransaction.Where(x => !x.IsSpent))
            {
                coinsToSpend.Add(coin.Id);
            }

            return true;
        }

        public async Task RevertExecutionAsync(
            IBalanceActionsRepository balanceActionsRepository,
            ITransactionsRepository transactionsRepository,
            ICoinsRepository coinsRepository)
        {
            if (!ExecutionCanBeReverted)
            {
                throw new InvalidOperationException($"Block can be reverted only in states: {BlockState.Executed} or {BlockState.PartiallyExecuted}, actual: {State}");
            }

            var removeBalanceActionsTask = balanceActionsRepository.TryRemoveAllOfBlockAsync(BlockchainType, Id);
            
            PaginatedItems<Transaction> transactions = null;

            do
            {
                transactions = await transactionsRepository.GetAllOfBlockAsync
                (
                    BlockchainType,
                    Id,
                    500,
                    transactions?.Continuation
                );

                var coinsToRevertSpending = transactions.Items
                    .Where(x => x.IsTransferCoins)
                    .Select(x => x.AsTransferCoins())
                    .SelectMany(t => t.SpentCoins)
                    .ToList();

                await coinsRepository.RevertSpendingAsync(BlockchainType, coinsToRevertSpending);

            } while (transactions.Continuation != null);
            
            await removeBalanceActionsTask;

            State = BlockState.RolledBack;
        }

        private async Task ExecuteTransactionAsync(
            IReadOnlyCollection<Coin> coinsToSpend,
            IBalanceActionsRepository balanceActionsRepository,
            IFeeEnvelopesRepository feeEnvelopesRepository,
            TransferCoinsExecutedTransaction transaction)
        {
            await Task.WhenAll
            (
                SaveBalanceActionsAsync(balanceActionsRepository, transaction, coinsToSpend),
                SaveFeesAsync(feeEnvelopesRepository, transaction, coinsToSpend)
            );
        }

        private async Task SaveBalanceActionsAsync(
            IBalanceActionsRepository balanceActionsRepository,
            TransferCoinsExecutedTransaction transaction,
            IReadOnlyCollection<Coin> coinsToSpend)
        {
            var receivedCoins = transaction.ReceivedCoins
                .Where(x => x.Address != null)
                .Select(x => new
                {
                    Address = x.Address,
                    Asset = x.Asset,
                    Value = (Money) x.Value
                });

            var actions = coinsToSpend
                .Where(x => 
                    x.Address != null && 
                    x.Address != Address.Unrecognized)
                .Select(x => new
                {
                    Address = x.Address,
                    Asset = x.Asset,
                    Value = -(Money) x.Value
                })
                .Concat(receivedCoins)
                .GroupBy(x => new {x.Address, x.Asset})
                .Select
                (
                    g => new BalanceAction
                    (
                        new AccountId(g.Key.Address, g.Key.Asset),
                        g.Sum(x => x.Value),
                        Number,
                        Id,
                        transaction.TransactionId
                    )
                );

            await balanceActionsRepository.AddIfNotExistsAsync(BlockchainType, actions);
        }

        private async Task SaveFeesAsync(
            IFeeEnvelopesRepository feeEnvelopesRepository,
            TransferCoinsExecutedTransaction transaction, 
            IReadOnlyCollection<Coin> coinsToSpend)
        {
            var assetReceivedAmount = transaction.ReceivedCoins
                .GroupBy(x => x.Asset)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Value));

            var assetSpentAmount = coinsToSpend
                .GroupBy(x => x.Asset)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Value));

            var fees = transaction.ReceivedCoins
                .Select(x => x.Asset)
                .Union(coinsToSpend.Select(x => x.Asset))
                .Select(asset =>
                {
                    assetReceivedAmount.TryGetValue(asset, out var receivedAmount);
                    assetSpentAmount.TryGetValue(asset, out var spentAmount);

                    var amount = spentAmount > receivedAmount
                        ? spentAmount - receivedAmount
                        : 0;

                    return new FeeEnvelope
                    (
                        BlockchainType,
                        Id,
                        transaction.TransactionId,
                        new Fee
                        (
                            asset,
                            amount
                        )
                    );
                });

            await feeEnvelopesRepository.AddIfNotExistsAsync(fees.ToList());
        }

        public override string ToString()
        {
            return $"{BlockchainType}:{Number}:{Id}({State}) <- {PreviousBlockId}";
        }
    }
}
