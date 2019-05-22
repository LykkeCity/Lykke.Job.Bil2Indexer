using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Contract.Events
{
    /// <summary>
    /// Batch of the transactions related to the block.
    /// </summary>
    [PublicAPI]
    [DataContract]
    public class TransactionsBatchEvent
    {
        /// <summary>
        /// Type of the blockchain.
        /// </summary>
        [DataMember(Order = 0)]
        public string BlockchainType { get; }

        /// <summary>
        /// ID of the block.
        /// </summary>
        [DataMember(Order = 1)]
        public string BlockId { get; }

        /// <summary>
        /// Number of the block.
        /// </summary>
        [DataMember(Order = 2)]
        public long BlockNumber { get; }

        /// <summary>
        /// Executed transactions.
        /// </summary>
        [DataMember(Order = 3)]
        public IReadOnlyCollection<ExecutedTransaction> ExecutedTransactions { get; }

        /// <summary>
        /// Failed transactions.
        /// </summary>
        [DataMember(Order = 4)]
        public IReadOnlyCollection<FailedTransaction> FailedTransactions { get; }

        /// <summary>
        /// Flag which indicates, if transactions in the batch are irreversible.
        /// </summary>
        [DataMember(Order = 5)]
        public bool IsIrreversible { get; }

        /// <summary>
        /// Batch of the transactions related to the block.
        /// </summary>
        /// <param name="blockchainType">Type of the blockchain.</param>
        /// <param name="blockId">ID of the block.</param>
        /// <param name="blockNumber">Number of the block.</param>
        /// <param name="executedTransactions">Executed transactions</param>
        /// <param name="failedTransactions">Failed transactions</param>
        /// <param name="isIrreversible">
        /// Flag which indicates, if transaction is irreversible.
        /// </param>
        public TransactionsBatchEvent(
            string blockchainType,
            BlockId blockId,
            long blockNumber,
            IReadOnlyCollection<ExecutedTransaction> executedTransactions,
            IReadOnlyCollection<FailedTransaction> failedTransactions,
            bool isIrreversible)
        {
            if (string.IsNullOrWhiteSpace(blockchainType))
                throw new ArgumentException("Should be not empty string", nameof(blockchainType));

            if (string.IsNullOrWhiteSpace(blockId))
                throw new ArgumentException("Should be not empty string", nameof(blockId));

            if (blockNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(blockNumber), blockNumber, "Should be zero or positive number");

            if (!ExecutedTransactions.Any() && !failedTransactions.Any())
                throw new ArgumentException("At least one executed or failed transaction should be passed to the batch");

            BlockchainType = blockchainType;
            BlockId = blockId;
            BlockNumber = blockNumber;
            IsIrreversible = isIrreversible;
            ExecutedTransactions = executedTransactions;
            FailedTransactions = failedTransactions;
        }
    }
}
