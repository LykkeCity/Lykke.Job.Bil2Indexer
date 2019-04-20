using System.Runtime.Serialization;
using MessagePack;

namespace Lykke.Job.Bil2Indexer.Workflow.Commands
{
    [DataContract]
    public class ExecuteTransferCoinsBlockCommand
    {
        [DataMember(Order = 0)]
        public string BlockchainType { get; set; }
        
        [DataMember(Order = 1)]
        public string BlockId { get; set; }

        [DataMember(Order = 2)]
        public long BlockVersion { get; set; }

        public override string ToString()
        {
            return $"{BlockchainType}:{BlockId}:{BlockVersion}";
        }
    }
}
