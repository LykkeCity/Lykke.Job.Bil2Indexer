using System;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Numerics;

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
            IBalanceActionsRepository balanceActionsRepository)
        {
            if (!CanBeExecuted)
            {
                throw new InvalidOperationException($"Block can be executed only in states: {BlockState.Assembled} or {BlockState.PartiallyExecuted}, actual: {State}");
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

                foreach (var transaction in transactions.Items)
                {
                    if (!await ExecuteTransactionAsync(coinsRepository, balanceActionsRepository, transaction))
                    {
                        State = BlockState.PartiallyExecuted;

                        return;
                    }
                }
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

                foreach (var transaction in transactions.Items)
                {
                    await RevertTransactionExecutionAsync(coinsRepository, transaction);
                }
            } while (transactions.Continuation != null);

            State = BlockState.RolledBack;
        }

        private async Task<bool> ExecuteTransactionAsync(
            ICoinsRepository coinsRepository,
            IBalanceActionsRepository balanceActionsRepository,
            TransferCoinsTransactionExecutedEvent transaction)
        {
            var coinsToSpend = await coinsRepository.GetSomeOfAsync(BlockchainType, transaction.SpentCoins);
            var missedCoins = transaction.SpentCoins.Except(coinsToSpend.Select(x => x.Id));

            if (missedCoins.Any())
            {
                return false;
            }

            foreach (var coin in coinsToSpend)
            {
                coin.SpendBy(transaction.TransactionId);
            }

            await coinsRepository.SaveAsync(coinsToSpend);

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

            return true;
        }

        private async Task RevertTransactionExecutionAsync(
            ICoinsRepository coinsRepository,
            TransferCoinsTransactionExecutedEvent transaction)
        {
            var coinsToSpend = await coinsRepository.GetSomeOfAsync(BlockchainType, transaction.SpentCoins);

            foreach (var coin in coinsToSpend)
            {
                coin.RevertSpendingBy(transaction.TransactionId);
            }

            await Task.WhenAll
            (
                coinsRepository.SaveAsync(coinsToSpend),
                coinsRepository.TryRemoveReceivedInTransactionAsync(BlockchainType, transaction.TransactionId)
            );
        }

        public override string ToString()
        {
            return $"{BlockchainType}:{Number}:{Id}({State}) <- {PreviousBlockId}";
        }
    }
}
