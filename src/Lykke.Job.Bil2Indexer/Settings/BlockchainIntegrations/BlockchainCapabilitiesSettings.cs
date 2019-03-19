using JetBrains.Annotations;

namespace Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations
{
    [UsedImplicitly]
    public class BlockchainCapabilitiesSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public BlockchainTransferModel TransferModel { get; set; }
    }
}
