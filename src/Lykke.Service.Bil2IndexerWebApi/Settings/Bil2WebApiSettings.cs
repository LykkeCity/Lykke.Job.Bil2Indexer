using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Service.Bil2IndexerWebApi.Settings.ApiSettings;
using Lykke.Service.Bil2IndexerWebApi.Settings.BlockchainIntegrations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.Bil2IndexerWebApi.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Bil2WebApiSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public DbSettings Db { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public IReadOnlyList<BlockchainIntegrationSettings> BlockchainIntegrations { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public AssetsCachingSettings AssetsCaching { get; set; }

        [Optional]
        public bool TelemetryEnabled { get; set; } = true;
    }
}
