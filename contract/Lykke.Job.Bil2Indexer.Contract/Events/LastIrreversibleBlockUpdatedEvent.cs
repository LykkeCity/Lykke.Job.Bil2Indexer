using System;
using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Contract.Events
{
    /// <summary>
    /// Published when last irreversible block number is updated.
    /// </summary>
    [PublicAPI]
    public class LastIrreversibleBlockUpdatedEvent
    {
        /// <summary>
        /// Type of the blockchain.
        /// </summary>
        public string BlockchainType { get; }

        /// <summary>
        /// Number of the last irreversible block.
        /// </summary>
        public long BlockNumber { get; }

        /// <summary>
        /// ID of the last irreversible block.
        /// </summary>
        public BlockId BlockId { get; }

        /// <summary>
        /// Should be published when last irreversible block number is updated.
        /// </summary>
        /// <param name="blockchainType">Type of the blockchain.</param>
        /// <param name="blockNumber">Number of the last irreversible block.</param>
        /// <param name="blockId">ID of the last irreversible block.</param>
        public LastIrreversibleBlockUpdatedEvent(
            string blockchainType,
            long blockNumber, 
            BlockId blockId)
        {
            if (string.IsNullOrWhiteSpace(blockchainType))
                throw new ArgumentException("Should be not empty string", nameof(blockchainType));

            if (blockNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(blockNumber), blockNumber, "Should be zero or positive number");

            BlockchainType = blockchainType;
            BlockNumber = blockNumber;
            BlockId = blockId ?? throw new ArgumentNullException(nameof(blockId));
        }
    }
}
