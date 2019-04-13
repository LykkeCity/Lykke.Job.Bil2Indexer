using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.Contract.Common;
using Lykke.Job.Bil2Indexer.Domain.Infrastructure;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Numerics;
using Lykke.Numerics.Linq;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class BlockHeader
    {
        public string Id { get; }
        public long Version { get; }
        public string BlockchainType { get; }
        public long Number { get; }
        public DateTime MinedAt { get; }
        public int Size { get; }
        public int TransactionsCount { get; }
        public string PreviousBlockId { get; }
        public BlockState State { get; private set; }

        public bool IsAssembled => State == BlockState.Assembled;

        public bool IsAlreadyAssembled =>
            State == BlockState.Assembled ||
            State == BlockState.Executed ||
            State == BlockState.PartiallyExecuted;

        public bool IsNotExecutedYet =>
            State == BlockState.Assembling ||
            State == BlockState.Assembled;

        public bool IsExecuted => State == BlockState.Executed;

        public bool IsPartiallyExecuted => State == BlockState.PartiallyExecuted;

        public bool CanBeExecuted => State == BlockState.Assembled || State == BlockState.PartiallyExecuted;

        public bool CanBeReverted => State == BlockState.Executed || State == BlockState.PartiallyExecuted;

        public BlockHeader(
            string id, 
            long version,
            string blockchainType, 
            long number, 
            DateTime minedAt, 
            int size,
            int transactionsCount, 
            string previousBlockId,
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
            string id, 
            string blockchainType, 
            long number, 
            DateTime minedAt, 
            int size,
            int transactionsCount, 
            string previousBlockId)
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
            IFeeEnvelopesRepository feeEnvelopesRepository)
        {
            if (!CanBeExecuted)
            {
                throw new InvalidOperationException($"Block can be executed only in states: {BlockState.Assembled} or {BlockState.PartiallyExecuted}, actual: {State}");
            }

            PaginatedItems<TransferCoinsTransactionExecutedEvent> transactions = null;
            var spendCoinsTask = default(Task);
            
            do
            {
                var coinsToSpend = new ConcurrentBag<CoinReference>();

                transactions = await transactionsRepository.GetTransferCoinsTransactionsOfBlockAsync
                (
                    BlockchainType,
                    Id,
                    transactions?.Continuation
                );

                var isAllTransactionsExecuted = await transactions.Items.ForEachAsync
                (
                    degreeOfParallelism: 8,
                    body: async transaction =>
                    {
                        var coinsToSpendByTransaction = await coinsRepository.GetSomeOfAsync(BlockchainType, transaction.SpentCoins);
                        var isExecuted = await ExecuteTransactionAsync
                        (
                            coinsToSpendByTransaction,
                            balanceActionsRepository,
                            feeEnvelopesRepository,
                            transaction
                        );

                        if (isExecuted)
                        {
                            foreach (var coin in coinsToSpendByTransaction.Where(x => !x.IsSpent))
                            {
                                coinsToSpend.Add(coin.Id);
                            }
                        }

                        return isExecuted;
                    });

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

            State = BlockState.Executed;
        }

        public async Task RevertExecutionAsync(
            ITransactionsRepository transactionsRepository,
            ICoinsRepository coinsRepository)
        {
            if (!CanBeReverted)
            {
                throw new InvalidOperationException($"Block can be reverted only in states: {BlockState.Executed} or {BlockState.PartiallyExecuted}, actual: {State}");
            }

            PaginatedItems<TransferCoinsTransactionExecutedEvent> transactions = null;

            do
            {
                transactions = await transactionsRepository.GetTransferCoinsTransactionsOfBlockAsync
                (
                    BlockchainType,
                    Id,
                    transactions?.Continuation
                );

                var coinsToRevertSpending = transactions.Items.SelectMany(t => t.SpentCoins);
                var transactionIds = transactions.Items.Select(t => t.TransactionId);

                await Task.WhenAll
                (
                    coinsRepository.RemoveIfExistAsync(BlockchainType, transactionIds),
                    coinsRepository.RevertSpendingAsync(BlockchainType, coinsToRevertSpending)
                );
                
            } while (transactions.Continuation != null);
            
            State = BlockState.RolledBack;
        }

        private async Task<bool> ExecuteTransactionAsync(
            IReadOnlyCollection<Coin> coinsToSpend,
            IBalanceActionsRepository balanceActionsRepository,
            IFeeEnvelopesRepository feeEnvelopesRepository,
            TransferCoinsTransactionExecutedEvent transaction)
        {
            var missedCoins = transaction.SpentCoins.Except(coinsToSpend.Select(x => x.Id));

            if (missedCoins.Any())
            {
                return false;
            }

            await Task.WhenAll
            (
                SaveBalanceActionsAsync(balanceActionsRepository, transaction, coinsToSpend),
                SaveFeesAsync(feeEnvelopesRepository, transaction, coinsToSpend)
            );

            return true;
        }

        private async Task SaveBalanceActionsAsync(
            IBalanceActionsRepository balanceActionsRepository,
            TransferCoinsTransactionExecutedEvent transaction,
            IReadOnlyCollection<Coin> coinsToSpend)
        {
            var actions = coinsToSpend
                .Where(c => c.Address != null)
                .Select
                (
                    x => new BalanceAction
                    (
                        x.Address,
                        x.Asset,
                        -(Money) x.Value,
                        Number,
                        Id,
                        transaction.TransactionId
                    )
                );

            await balanceActionsRepository.SaveAsync(BlockchainType, actions);
        }

        private async Task SaveFeesAsync(
            IFeeEnvelopesRepository feeEnvelopesRepository,
            TransferCoinsTransactionExecutedEvent transaction, 
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

            await feeEnvelopesRepository.SaveAsync(fees);
        }

        public override string ToString()
        {
            return $"{BlockchainType}:{Number}:{Id}({State}) <- {PreviousBlockId}";
        }
    }
}
