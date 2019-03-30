using System;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class BlockHeader
    {
        public string BlockchainType { get; }
        public long Number { get; }
        public string Id { get; }
        public DateTime MiningMoment { get; }
        public int Size { get; }
        public int TransactionsCount { get; }
        public string PreviousBlockId { get; }

        public BlockHeader(string blockchainType, long number, string id, DateTime miningMoment, int size, int transactionsCount, string previousBlockId)
        {
            BlockchainType = blockchainType;
            Number = number;
            Id = id;
            MiningMoment = miningMoment;
            Size = size;
            TransactionsCount = transactionsCount;
            PreviousBlockId = previousBlockId;
        }

        public override string ToString()
        {
            return $"{BlockchainType}:{Number}:{Id} <- {PreviousBlockId}";
        }
    }
}
