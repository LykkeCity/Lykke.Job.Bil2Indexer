using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.Bil2Indexer.Settings.JobSettings
{
    [UsedImplicitly]
    public class DbSettings
    {
        [AzureTableCheck]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string LogsConnString { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string PgBlockchainDataConnString { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string PgStateDataConnString { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string PgTransactionsDataConnString { get; set; }
    }
}
