using JetBrains.Annotations;

namespace Lykke.Service.Bil2IndexerGrpcApi.Settings
{
    [UsedImplicitly]
    public class Bil2IndexerGrpcApiSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public DbSettings Db { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public RabbitMqSettings RabbitMq { get; set; }
    }
}
