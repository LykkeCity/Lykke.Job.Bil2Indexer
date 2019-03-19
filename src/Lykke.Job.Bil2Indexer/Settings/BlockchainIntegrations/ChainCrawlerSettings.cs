using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations
{
    [UsedImplicitly]
    public class ChainCrawlerSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public long StartBlock { get; set; }

        [Optional]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public long? StopBlock { get; set; }
    }
}