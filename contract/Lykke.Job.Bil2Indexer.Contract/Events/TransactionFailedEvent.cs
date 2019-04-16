using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Contract.Events
{
    /// <summary>
    /// Event indicating that transaction included in a block as filed.
    /// </summary>
    [PublicAPI]
    public class TransactionFailedEvent
    {
        /// <summary>
        /// Type of the blockchain.
        /// </summary>
        public string BlockchainType { get; }

        /// <summary>
        /// ID of the block.
        /// </summary>
        public string BlockId { get; }

        /// <summary>
        /// Number of the block.
        /// </summary>
        public long BlockNumber { get; }

        /// <summary>
        /// One-based number of the transaction in the block.
        /// </summary>
        public int TransactionNumber { get; }

        /// <summary>
        /// ID of the transaction.
        /// </summary>
        public string TransactionId { get; }

        /// <summary>
        /// Code of the error.
        /// </summary>
        public TransactionBroadcastingError ErrorCode { get; }

        /// <summary>
        /// Clean error description.
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Optional.
        /// Fees in the particular asset, that was spent for the transaction.
        /// Can be omitted, if there was no fee spent for the transaction.
        /// </summary>
        [CanBeNull]
        public IReadOnlyCollection<Fee> Fees { get; }

        /// <summary>
        /// Should be published for each failed transaction in the block being read.
        /// </summary>
        /// <param name="blockchainType">Type of the blockchain.</param>
        /// <param name="blockId">ID of the block.</param>
        /// <param name="blockNumber">Number of the block.</param>
        /// <param name="transactionNumber">One-based number of the transaction in the block.</param>
        /// <param name="transactionId">ID of the transaction.</param>
        /// <param name="errorCode">Code of the error.</param>
        /// <param name="errorMessage">
        /// Optional.
        /// Fee in the particular asset ID, that was spent for the transaction.
        /// Can be omitted, if there was no fee spent for the transaction.
        /// </param>
        /// <param name="fees">
        /// Optional.
        /// Fees in the particular asset, that was spent for the transaction.
        /// Can be omitted, if there was no fee spent for the transaction.
        /// </param>
        public TransactionFailedEvent(
            string blockchainType,
            string blockId,
            long blockNumber,
            int transactionNumber,
            string transactionId,
            TransactionBroadcastingError errorCode,
            string errorMessage,
            IReadOnlyCollection<Fee> fees = null)
        {
            if (string.IsNullOrWhiteSpace(blockchainType))
                throw new ArgumentException("Should be not empty string", nameof(blockchainType));

            if (string.IsNullOrWhiteSpace(blockId))
                throw new ArgumentException("Should be not empty string", nameof(blockId));

            if (blockNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(blockNumber), blockNumber, "Should be zero or positive number");

            if (transactionNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(transactionNumber), transactionNumber, "Should be zero or positive number");

            if (string.IsNullOrWhiteSpace(transactionId))
                throw new ArgumentException("Should be not empty string", nameof(transactionId));

            BlockchainType = blockchainType;
            BlockId = blockId;
            BlockNumber = blockNumber;
            TransactionNumber = transactionNumber;
            TransactionId = transactionId;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            Fees = fees;
        }
    }
}
