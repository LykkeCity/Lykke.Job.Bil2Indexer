namespace Lykke.Job.Bil2Indexer.Models.Management
{
    public class ReadBlockRequest
    {
        public string CorrelationId { get; set; }
        public string BlockchainType { get; set; }
        public long BlockNumber { get; set; }
    }
}
