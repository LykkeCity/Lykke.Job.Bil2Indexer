using JetBrains.Annotations;

namespace Lykke.Service.Bil2IndexerWebApi.Settings.BlockchainIntegrations
{
    [UsedImplicitly]
    public class BlockchainIntegrationSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string Type { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string PgBlockchainDataConnString { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string PgStateDataConnString { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string PgTransactionsDataConnString { get; set; }
    }
}
