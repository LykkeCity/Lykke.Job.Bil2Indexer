using JetBrains.Annotations;

namespace Lykke.Job.Bil2Indexer.Settings.JobSettings
{
    [UsedImplicitly]
    public class AssetsCachingSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public int LruCacheCapacity { get; set; }
    }
}
