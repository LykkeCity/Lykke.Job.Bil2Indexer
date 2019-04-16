using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.Bil2Indexer.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Bil2IndexerSettings
    {
        public DbSettings Db { get; set; }
    }
}
