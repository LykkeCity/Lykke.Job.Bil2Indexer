using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Service.Bil2IndexerGrpcApi.Settings
{
    [UsedImplicitly]
    public class AppSettings : BaseAppSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public Bil2IndexerGrpcApiSettings Bil2IndexerGrpcApi { get; set; }
    }
}
