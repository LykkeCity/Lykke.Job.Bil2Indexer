using JetBrains.Annotations;

namespace Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations
{
    [UsedImplicitly]
    public class BlockchainIntegrationSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string Type { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public BlockchainCapabilitiesSettings Capabilities { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public BlockchainIndexerSettings Indexer { get; set; }
    }
}
