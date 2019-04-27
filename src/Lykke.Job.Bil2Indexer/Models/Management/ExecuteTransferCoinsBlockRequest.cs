namespace Lykke.Job.Bil2Indexer.Models.Management
{
    public class ExecuteTransferCoinsBlockRequest
    {
        public string CorrelationId { get; set; }
        public string BlockchainType { get; set; }
        public string BlockId { get; set; }
    }
}
