using System;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class ChainHead
    {
        public string BlockchainType { get; }
        public long Version { get; }
        public long FirstBlockNumber { get; }
        public long? BlockNumber { get; private set; }
        public BlockId BlockId { get; private set; }
        public BlockId PreviousBlockId { get; private set; }
        public long Sequence { get; private set; }

        public ChainHead(
            string blockchainType,
            long firstBlockNumber,
            long version,
            long sequence,
            long? blockNumber,
            BlockId blockId,
            BlockId previousBlockId)
        {
            BlockchainType = blockchainType;
            FirstBlockNumber = firstBlockNumber;
            Version = version;
            Sequence = sequence;
            BlockNumber = blockNumber;
            BlockId = blockId;
            PreviousBlockId = previousBlockId;
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
                sequence: 0,
                blockNumber: null,
                blockId: null,
                previousBlockId: null
            );
        }

        public void ExtendTo(long blockNumber, BlockId blockId)
        {
            if (!CanExtendTo(blockNumber))
            {
                throw new InvalidOperationException($"Chain head can't be extended to the block {blockNumber}:{blockId}. Expected block number {BlockNumber + 1}. Current block is {BlockNumber}");
            }

            BlockNumber = blockNumber;
            PreviousBlockId = BlockId;
            BlockId = blockId;
            ++Sequence;
        }

        public void ReduceTo(long blockNumber, BlockId blockId)
        {
            if (!CanReduceTo(blockNumber))
            {
                throw new InvalidOperationException($"Chain head can't be reduced to the block {blockNumber}:{blockId}. Expected block number {BlockNumber - 1}. Current block is {BlockNumber}. First block number {FirstBlockNumber}");
            }

            BlockNumber = blockNumber;
            PreviousBlockId = BlockId;
            BlockId = blockId;
            ++Sequence;
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

        public ChainHeadCorrelationId GetCorrelationId()
        {
            return new ChainHeadCorrelationId(BlockchainType, Sequence);
        }

        public override string ToString()
        {
            return $"{BlockchainType}:{BlockNumber}({Version})";
        }
    }
}
