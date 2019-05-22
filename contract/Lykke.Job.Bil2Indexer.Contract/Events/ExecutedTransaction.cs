using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Contract.Events
{
    /// <summary>
    /// Executed transaction.
    /// </summary>
    [PublicAPI]
    [DataContract]
    public class ExecutedTransaction
    {
        /// <summary>
        /// Number of the transaction in the block.
        /// </summary>
        [DataMember(Order = 0)]
        public int TransactionNumber { get; }

        /// <summary>
        /// ID of the transaction.
        /// </summary>
        [DataMember(Order = 1)]
        public string TransactionId { get; }

        /// <summary>
        /// Balance updating operations.
        /// </summary>
        [DataMember(Order = 2)]
        public IReadOnlyCollection<BalanceUpdate> BalanceUpdates { get; }

        /// <summary>
        /// Fees in the particular asset, that was spent for the transaction.
        /// </summary>
        [DataMember(Order = 3)]
        public IReadOnlyCollection<Fee> Fees { get; }

        /// <summary>
        /// "Transfer amount" transactions model.
        /// Should be published for each executed transaction in the block being read if
        /// integration uses “transfer amount” transactions model. Integration should either
        /// support “transfer coins” or “transfer amount” transactions model.
        /// </summary>
        /// <param name="transactionNumber">Number of the transaction in the block.</param>
        /// <param name="transactionId">ID of the transaction.</param>
        /// <param name="balanceUpdates">Balance changing operations.</param>
        /// <param name="fees">Fees in the particular asset, that was spent for the transaction.</param>
        public ExecutedTransaction(
            int transactionNumber,
            string transactionId,
            IReadOnlyCollection<BalanceUpdate> balanceUpdates,
            IReadOnlyCollection<Fee> fees)
        {
            if (transactionNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(transactionNumber), transactionNumber, "Should be zero or positive number");

            if (string.IsNullOrWhiteSpace(transactionId))
                throw new ArgumentException("Should be not empty string", nameof(transactionId));

            TransactionNumber = transactionNumber;
            TransactionId = transactionId;
            BalanceUpdates = balanceUpdates ?? throw new ArgumentNullException(nameof(balanceUpdates));
            Fees = fees ?? throw new ArgumentNullException(nameof(fees));
        }
    }
}
