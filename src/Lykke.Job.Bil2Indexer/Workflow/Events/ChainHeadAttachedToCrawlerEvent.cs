using System.Runtime.Serialization;

namespace Lykke.Job.Bil2Indexer.Workflow.Events
{
    [DataContract]
    public class ChainHeadAttachedToCrawlerEvent
    {
        [DataMember(Order = 0)]
        public string BlockchainType { get; set; }
    }
}
