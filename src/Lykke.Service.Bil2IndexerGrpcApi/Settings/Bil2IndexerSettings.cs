using JetBrains.Annotations;

namespace Lykke.Service.Bil2IndexerGrpcApi.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Bil2IndexerSettings
    {
        public DbSettings Db { get; set; }
    }
}
