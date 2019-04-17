using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Service.Bil2IndexerGrpcApi.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public Bil2IndexerSettings Bil2IndexerService { get; set; }
    }
}
