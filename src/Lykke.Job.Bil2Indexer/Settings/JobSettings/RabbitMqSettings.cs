using System;
using JetBrains.Annotations;

namespace Lykke.Job.Bil2Indexer.Settings.JobSettings
{
    [UsedImplicitly]
    public class RabbitMqSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string ConnString { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public TimeSpan DefaultRetryDelay { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public int ListeningParallelism { get; set; }
    }
}
