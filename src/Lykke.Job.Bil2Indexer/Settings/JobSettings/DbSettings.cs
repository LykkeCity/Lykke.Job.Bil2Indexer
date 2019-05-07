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
    }
}
