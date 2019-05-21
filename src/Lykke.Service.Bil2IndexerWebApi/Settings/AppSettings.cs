using JetBrains.Annotations;
using Lykke.Sdk.Settings;
using Lykke.Service.Bil2IndexerWebApi.Settings.ApiSettings;

namespace Lykke.Service.Bil2IndexerWebApi.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public Bil2WebApiSettings Bil2WebApiService { get; set; }
    }
}
