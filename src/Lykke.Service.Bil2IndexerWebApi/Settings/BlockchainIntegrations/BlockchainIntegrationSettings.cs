using JetBrains.Annotations;

namespace Lykke.Service.Bil2IndexerWebApi.Settings.BlockchainIntegrations
{
    [UsedImplicitly]
    public class BlockchainIntegrationSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string Type { get; set; }
        public string PgConnectionString { get; set; }
    }
}
