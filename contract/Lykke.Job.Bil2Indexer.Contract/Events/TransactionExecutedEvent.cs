using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Contract.Events
{
    [PublicAPI]
    public class TransactionExecutedEvent
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
        /// Number of the transaction in the block.
        /// </summary>
        public int TransactionNumber { get; }

        /// <summary>
        /// ID of the transaction.
        /// </summary>
        public string TransactionId { get; }

        /// <summary>
        /// Balance updating operations.
        /// </summary>
        public IReadOnlyCollection<BalanceUpdate> BalanceUpdates { get; }

        /// <summary>
        /// Fees in the particular asset, that was spent for the transaction.
        /// </summary>
        public IReadOnlyCollection<Fee> Fees { get; }

        /// <summary>
        /// Optional.
        /// Flag which indicates, if transaction is irreversible.
        /// </summary>
        [CanBeNull]
        public bool? IsIrreversible { get; }

        /// <summary>
        /// "Transfer amount" transactions model.
        /// Should be published for each executed transaction in the block being read if
        /// integration uses “transfer amount” transactions model. Integration should either
        /// support “transfer coins” or “transfer amount” transactions model.
        /// </summary>
        /// <param name="blockchainType">Type of the blockchain.</param>
        /// <param name="blockId">ID of the block.</param>
        /// <param name="blockNumber">Number of the block.</param>
        /// <param name="transactionNumber">Number of the transaction in the block.</param>
        /// <param name="transactionId">ID of the transaction.</param>
        /// <param name="balanceUpdates">Balance changing operations.</param>
        /// <param name="fees">Fees in the particular asset, that was spent for the transaction.</param>
        /// <param name="isIrreversible">
        /// Optional.
        /// Flag which indicates, if transaction is irreversible.
        /// </param>
        public TransactionExecutedEvent(
            string blockchainType, 
            string blockId,
            long blockNumber,
            int transactionNumber,
            string transactionId,
            IReadOnlyCollection<BalanceUpdate> balanceUpdates,
            IReadOnlyCollection<Fee> fees,
            bool? isIrreversible = null)
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
            BalanceUpdates = balanceUpdates ?? throw new ArgumentNullException(nameof(balanceUpdates));
            Fees = fees ?? throw new ArgumentNullException(nameof(fees));
            IsIrreversible = isIrreversible;
        }
    }
}
