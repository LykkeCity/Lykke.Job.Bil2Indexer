using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.Bil2Indexer.Settings.JobSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
