namespace Lykke.Job.Bil2Indexer.Domain
{
    public class BlockBuilding
    {
        public string Id { get; }
        public int TotalTransactionsNumber { get; }
        public int ReceivedTransactionsNumber { get; }
        
        public BlockBuilding(string id, int totalTransactionsNumber, int receivedTransactionsNumber)
        {
            Id = id;
            TotalTransactionsNumber = totalTransactionsNumber;
            ReceivedTransactionsNumber = receivedTransactionsNumber;
        }
    }
}
