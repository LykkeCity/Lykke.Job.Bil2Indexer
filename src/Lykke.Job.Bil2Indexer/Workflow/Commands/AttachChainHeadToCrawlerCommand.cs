using System.Runtime.Serialization;

namespace Lykke.Job.Bil2Indexer.Workflow.Commands
{
    [DataContract]
    public class AttachChainHeadToCrawlerCommand
    {
        [DataMember(Order = 0)]
        public string BlockchainType { get; set; }

        [DataMember(Order = 1)]
        public long CrawlerSequence { get; set; }
    }
}
