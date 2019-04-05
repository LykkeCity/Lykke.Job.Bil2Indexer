﻿namespace Lykke.Job.Bil2Indexer.Workflow.Commands
{
    public class ReduceChainHeadCommand
    {
        public string BlockchainType { get; set; }
        public long ToBlockNumber { get; set; }
        public string ToBlockId { get; set; }
        public string BlockIdToRollback { get; set; }
        public long ChainHeadVersion { get; set; }

        public override string ToString()
        {
            return $"{BlockchainType}:{ToBlockNumber}({ChainHeadVersion})";
        }
    }
}
