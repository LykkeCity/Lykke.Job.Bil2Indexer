using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations;
using Lykke.Job.Bil2Indexer.Settings.JobSettings;
using Lykke.Sdk.Settings;

namespace Lykke.Job.Bil2Indexer.Settings
{
    [UsedImplicitly]
    public class AppSettings : BaseAppSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public Bil2IndexerJobSettings Bil2IndexerJob { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public IReadOnlyList<BlockchainIntegrationSettings> BlockchainIntegrations { get; set; }
    }
}
