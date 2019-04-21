using System;
using Lykke.Bil2.Contract.BlocksReader.Events;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class TransactionEnvelope
    {
        public bool IsTransferCoins => _transferCoinsTransaction != null;
        public bool IsTransferAmount => _transactionAmountTransaction != null;
        public bool IsFailed => _failedTransaction != null;

        private readonly TransferAmountTransactionExecutedEvent _transactionAmountTransaction;
        private readonly TransferCoinsTransactionExecutedEvent _transferCoinsTransaction;
        private readonly TransactionFailedEvent _failedTransaction;

        public TransactionEnvelope(TransferCoinsTransactionExecutedEvent transaction)
        {
            _transferCoinsTransaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        public TransactionEnvelope(TransferAmountTransactionExecutedEvent transaction)
        {
            _transactionAmountTransaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        public TransactionEnvelope(TransactionFailedEvent transaction)
        {
            _failedTransaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
        }

        public TransferCoinsTransactionExecutedEvent AsTransferCoins()
        {
            if (!IsTransferCoins)
            {
                throw new InvalidOperationException($"This transaction is not a 'transfer coins' transaction. Actual type: {GetTransactionTypeText()}");
            }

            return _transferCoinsTransaction;
        }

        public TransferAmountTransactionExecutedEvent AsTransferAmount()
        {
            if (!IsTransferAmount)
            {
                throw new InvalidOperationException($"This transaction is not a 'transfer amount' transaction. Actual type: {GetTransactionTypeText()}");
            }

            return _transactionAmountTransaction;
        }

        public TransactionFailedEvent AsFailed()
        {
            if (!IsFailed)
            {
                throw new InvalidOperationException($"This transaction is not a failed transaction. Actual type: {GetTransactionTypeText()}");
            }

            return _failedTransaction;
        }

        public TransferCoinsTransactionExecutedEvent AsTransferCoinsOrDefault()
        {
            return _transferCoinsTransaction;
        }

        public TransferAmountTransactionExecutedEvent AsTransferAmountOrDefault()
        {
            return _transactionAmountTransaction;
        }

        public TransactionFailedEvent AsFailedOrDefault()
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
