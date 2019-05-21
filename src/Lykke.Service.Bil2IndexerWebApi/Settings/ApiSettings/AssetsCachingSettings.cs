using JetBrains.Annotations;

namespace Lykke.Service.Bil2IndexerWebApi.Settings.ApiSettings
{
    [UsedImplicitly]
    public class AssetsCachingSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public int LruCacheCapacity { get; set; }
    }
}
