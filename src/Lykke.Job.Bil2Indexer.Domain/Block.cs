namespace Lykke.Job.Bil2Indexer.Domain
{
    public class Block
    {
        public string Hash { get; }
        public int ReceivedTransactionsNumber { get; }
        public BlockState State { get; }

        public Block(string hash, int receivedTransactionsNumber, BlockState state)
        {
            Hash = hash;
            ReceivedTransactionsNumber = receivedTransactionsNumber;
            State = state;
        }
    }
}
