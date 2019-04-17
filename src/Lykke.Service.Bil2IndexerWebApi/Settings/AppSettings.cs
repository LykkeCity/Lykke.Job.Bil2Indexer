using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Service.Bil2IndexerWebApi.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public Bil2IndexerSettings Bil2IndexerService { get; set; }
    }
}
