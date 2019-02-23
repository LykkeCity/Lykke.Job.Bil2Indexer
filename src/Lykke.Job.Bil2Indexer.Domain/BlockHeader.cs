using System;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class BlockHeader
    {
        public long Number { get; }
        public string Hash { get; }
        public DateTime MiningMoment { get; }
        public int Size { get; }
        public int TransactionsNumber { get; }
        public string PreviousBlockHash { get; }

        public BlockHeader(long number, string hash, DateTime miningMoment, int size, int transactionsNumber, string previousBlockHash)
        {
            Number = number;
            Hash = hash;
            MiningMoment = miningMoment;
            Size = size;
            TransactionsNumber = transactionsNumber;
            PreviousBlockHash = previousBlockHash;
        }

        public override string ToString()
        {
            return $"{Number}: {PreviousBlockHash} -> {Hash}";
        }
    }
}
