using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.Bil2IndexerGrpcApi.Settings
{
    [UsedImplicitly]
    public class DbSettings
    {
        [AzureTableCheck]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string LogsConnString { get; set; }
    }
}
