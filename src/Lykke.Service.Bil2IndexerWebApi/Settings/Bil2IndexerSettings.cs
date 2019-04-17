using JetBrains.Annotations;

namespace Lykke.Service.Bil2IndexerWebApi.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Bil2IndexerSettings
    {
        public DbSettings Db { get; set; }
    }
}
