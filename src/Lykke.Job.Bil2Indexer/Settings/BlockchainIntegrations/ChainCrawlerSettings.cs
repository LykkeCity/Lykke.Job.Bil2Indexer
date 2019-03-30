using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations
{
    [UsedImplicitly]
    public class ChainCrawlerSettings
    {
        /// <summary>
        /// Inclusive
        /// </summary>
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public long StartBlock { get; set; }

        /// <summary>
        /// Exclusive
        /// </summary>
        [Optional]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public long? StopBlock { get; set; }
    }
}
