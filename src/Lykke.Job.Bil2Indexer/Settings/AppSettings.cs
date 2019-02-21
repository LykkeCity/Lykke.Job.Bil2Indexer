using Lykke.Job.Bil2Indexer.Settings.JobSettings;
using Lykke.Sdk.Settings;

namespace Lykke.Job.Bil2Indexer.Settings
{
    public class AppSettings : BaseAppSettings
    {
        public Bil2IndexerJobSettings Bil2IndexerJob { get; set; }
    }
}
