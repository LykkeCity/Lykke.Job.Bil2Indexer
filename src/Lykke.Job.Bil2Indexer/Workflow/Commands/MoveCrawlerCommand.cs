using System.Runtime.Serialization;

namespace Lykke.Job.Bil2Indexer.Workflow.Commands
{
    [DataContract]
    public class MoveCrawlerCommand
    {
        [DataMember(Order = 0)]
        public string BlockchainType { get; set; }
        
        [DataMember(Order = 1)]
        public long NextBlockNumber { get; set; }

        public override string ToString()
        {
            return $"{BlockchainType}:{NextBlockNumber}";
        }
    }
}
