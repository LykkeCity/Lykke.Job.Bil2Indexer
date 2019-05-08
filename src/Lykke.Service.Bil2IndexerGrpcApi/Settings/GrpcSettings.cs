using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.Bil2IndexerGrpcApi.Settings
{
    public class GrpcSettings
    {
        [Optional]
        public int Port { get; set; } = 5100;
    }
}
