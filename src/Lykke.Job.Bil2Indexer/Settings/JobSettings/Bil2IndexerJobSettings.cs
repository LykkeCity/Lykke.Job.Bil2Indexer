using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations;

namespace Lykke.Job.Bil2Indexer.Settings.JobSettings
{
    [UsedImplicitly]
    public class Bil2IndexerJobSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public DbSettings Db { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public RabbitMqSettings RabbitMq { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public AssetsCachingSettings AssetsCaching { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public BlocksAssemblingSettings BlocksAssembling { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public IReadOnlyList<BlockchainIntegrationSettings> BlockchainIntegrations { get; set; }
    }
}
