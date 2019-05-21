using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Contract.Events
{
    /// <summary>
    /// Failed transaction.
    /// </summary>
    [PublicAPI]
    [DataContract]
    public class FailedTransaction
    {
        /// <summary>
        /// One-based number of the transaction in the block.
        /// </summary>
        [DataMember(Order = 0)]
        public int TransactionNumber { get; }

        /// <summary>
        /// ID of the transaction.
        /// </summary>
        [DataMember(Order = 1)]
        public string TransactionId { get; }

        /// <summary>
        /// Code of the error.
        /// </summary>
        [DataMember(Order = 2)]
        public TransactionBroadcastingError ErrorCode { get; }

        /// <summary>
        /// Clean error description.
        /// </summary>
        [DataMember(Order = 3)]
        public string ErrorMessage { get; }

        /// <summary>
        /// Optional.
        /// Fees in the particular asset, that was spent for the transaction.
        /// Can be omitted, if there was no fee spent for the transaction.
        /// </summary>
        [CanBeNull]
        [DataMember(Order = 4)]
        public IReadOnlyCollection<Fee> Fees { get; }

        /// <summary>
        /// Should be published for each failed transaction in the block being read.
        /// </summary>
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
        public FailedTransaction(
            int transactionNumber,
            string transactionId,
            TransactionBroadcastingError errorCode,
            string errorMessage,
            IReadOnlyCollection<Fee> fees = null)
        {
            if (transactionNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(transactionNumber), transactionNumber, "Should be zero or positive number");

            if (string.IsNullOrWhiteSpace(transactionId))
                throw new ArgumentException("Should be not empty string", nameof(transactionId));

            TransactionNumber = transactionNumber;
            TransactionId = transactionId;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            Fees = fees;
        }
    }
}
