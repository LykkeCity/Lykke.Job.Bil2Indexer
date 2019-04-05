using System;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class ChainHead
    {
        public string BlockchainType { get; }
        public long Version { get; }
        public long FirstBlockNumber { get; }
        public long? BlockNumber { get; private set; }
        public string BlockId { get; private set; }

        public ChainHead(
            string blockchainType,
            long firstBlockNumber,
            long version,
            long? blockNumber,
            string blockId)
        {
            BlockchainType = blockchainType;
            FirstBlockNumber = firstBlockNumber;
            Version = version;
            BlockNumber = blockNumber;
            BlockId = blockId;
        }

        public static ChainHead CreateNew(
            string blockchainType,
            long firstBlockNumber)
        {
            return new ChainHead
            (
                blockchainType: blockchainType,
                firstBlockNumber: firstBlockNumber,
                version: 0,
                blockNumber: null,
                blockId: null
            );
        }

        public void ExtendTo(long blockNumber, string blockId)
        {
            if (!CanExtendTo(blockNumber))
            {
                throw new InvalidOperationException($"Chain head can't be extended to the block {blockNumber}:{blockId}. Expected block number {BlockNumber + 1}. Current block is {BlockNumber}");
            }

            BlockNumber = blockNumber;
            BlockId = blockId;
        }

        public void ReduceTo(long blockNumber, string blockId)
        {
            if (!CanReduceTo(blockNumber))
            {
                throw new InvalidOperationException($"Chain head can't be reduced to the block {blockNumber}:{blockId}. Expected block number {BlockNumber - 1}. Current block is {BlockNumber}. First block number {FirstBlockNumber}");
            }

            BlockNumber = blockNumber;
            BlockId = blockId;
        }

        public bool CanExtendTo(long blockNumber)
        {
            if (!BlockNumber.HasValue && FirstBlockNumber == blockNumber)
            {
                return true;
            }

            if (BlockNumber.HasValue && BlockNumber == blockNumber - 1)
            {
                return true;
            }

            return false;
        }

        public bool CanReduceTo(long blockNumber)
        {
            if (BlockNumber.HasValue && BlockNumber == blockNumber + 1 && blockNumber >= FirstBlockNumber)
            {
                return true;
            }

            return false;
        }

        public bool IsOnBlock(long blockNumber)
        {
            return BlockNumber == blockNumber;
        }

        public override string ToString()
        {
            return $"{BlockchainType}:{BlockNumber}({Version})";
        }
    }
}
