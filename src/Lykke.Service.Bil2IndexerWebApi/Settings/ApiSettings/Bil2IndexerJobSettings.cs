using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Service.Bil2IndexerWebApi.Settings.BlockchainIntegrations;

namespace Lykke.Service.Bil2IndexerWebApi.Settings.ApiSettings
{
    [UsedImplicitly]
    public class Bil2IndexerJobSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public DbSettings Db { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public AssetsCachingSettings AssetsCaching { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public IReadOnlyList<BlockchainIntegrationSettings> BlockchainIntegrations { get; set; }
    }
}
