using System.Runtime.Serialization;

namespace Lykke.Job.Bil2Indexer.Workflow.Events
{
    [DataContract]
    public class CrawlerMovedEvent
    {
        [DataMember(Order = 0)]
        public string BlockchainType { get; set; }
        
        [DataMember(Order = 1)]
        public long BlockNumber { get; set; }
    }
}
