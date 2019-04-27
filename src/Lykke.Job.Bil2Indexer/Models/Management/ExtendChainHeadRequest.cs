namespace Lykke.Job.Bil2Indexer.Models.Management
{
    public class ExtendChainHeadRequest
    {
        public string CorrelationId { get; set; }
        public string BlockchainType { get; set; }
        public long ToBlockNumber { get; set; }
        public string ToBlockId { get; set; }
    }
}
