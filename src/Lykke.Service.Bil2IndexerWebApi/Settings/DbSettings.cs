using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.Bil2IndexerWebApi.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
