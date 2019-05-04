using System;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class Transaction
    {
        public string BlockchainType { get; }
        public BlockId BlockId { get; }
        public TransactionType Type { get; }

        public bool IsTransferCoins => Type == TransactionType.TransferCoins;
        public bool IsTransferAmount => Type == TransactionType.TransferAmount;
        public bool IsFailed => Type == TransactionType.Failed;

        private readonly TransferAmountExecutedTransaction _transactionAmountTransaction;
        private readonly TransferCoinsExecutedTransaction _transferCoinsTransaction;
        private readonly FailedTransaction _failedTransaction;

        public Transaction(string blockchainType, BlockId blockId, TransferCoinsExecutedTransaction transaction)
        {
            if (string.IsNullOrWhiteSpace(blockchainType))
            {
                throw new ArgumentException("Should be not empty string", nameof(blockchainType));
            }

            BlockchainType = blockchainType;
            BlockId = blockId ?? throw new ArgumentNullException(nameof(blockId));
            Type = TransactionType.TransferCoins;
            _transferCoinsTransaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        public Transaction(string blockchainType, BlockId blockId, TransferAmountExecutedTransaction transaction)
        {
            if (string.IsNullOrWhiteSpace(blockchainType))
            {
                throw new ArgumentException("Should be not empty string", nameof(blockchainType));
            }

            BlockchainType = blockchainType;
            BlockId = blockId ?? throw new ArgumentNullException(nameof(blockId));
            Type = TransactionType.TransferAmount;
            _transactionAmountTransaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        public Transaction(string blockchainType, BlockId blockId, FailedTransaction transaction)
        {
            if (string.IsNullOrWhiteSpace(blockchainType))
            {
                throw new ArgumentException("Should be not empty string", nameof(blockchainType));
            }

            BlockchainType = blockchainType;
            BlockId = blockId ?? throw new ArgumentNullException(nameof(blockId));
            Type = TransactionType.Failed;
            _failedTransaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        public TransferCoinsExecutedTransaction AsTransferCoins()
        {
            if (!IsTransferCoins)
            {
                throw new InvalidOperationException($"This transaction is not a 'transfer coins' transaction. Actual type: {GetTransactionTypeText()}");
            }

            return _transferCoinsTransaction;
        }

        public TransferAmountExecutedTransaction AsTransferAmount()
        {
            if (!IsTransferAmount)
            {
                throw new InvalidOperationException($"This transaction is not a 'transfer amount' transaction. Actual type: {GetTransactionTypeText()}");
            }

            return _transactionAmountTransaction;
        }

        public FailedTransaction AsFailed()
        {
            if (!IsFailed)
            {
                throw new InvalidOperationException($"This transaction is not a failed transaction. Actual type: {GetTransactionTypeText()}");
            }

            return _failedTransaction;
        }

        public TransferCoinsExecutedTransaction AsTransferCoinsOrDefault()
        {
            return _transferCoinsTransaction;
        }

        public TransferAmountExecutedTransaction AsTransferAmountOrDefault()
        {
            return _transactionAmountTransaction;
        }

        public FailedTransaction AsFailedOrDefault()
        {
            return _failedTransaction;
        }

        private string GetTransactionTypeText()
        {
            if (IsTransferCoins)
            {
                return "transfer coins";
            }

            if (IsTransferAmount)
            {
                return "transfer amount";
            }

            if (IsFailed)
            {
                return "failed";
            }

            throw new InvalidOperationException("Unknown transaction type");
        }
    }
}
