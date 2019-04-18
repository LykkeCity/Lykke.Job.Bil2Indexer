using System;
using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.Bil2IndexerGrpcApi.Settings
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

        [Optional]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string Vhost { get; set; }
    }
}
