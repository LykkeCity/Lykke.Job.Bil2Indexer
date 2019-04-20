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

        /// <summary>
        /// RabbitMq Vhost name.
        /// </summary>
        [Optional]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string Vhost { get; set; }

        /// <summary>
        /// Number of the threads used to listen messages from the RabbitMq.
        /// </summary>
        [Optional]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public int MessageConsumersCount { get; set; } = 4;

        /// <summary>
        /// Number of the threads used to process messages from the RabbitMq.
        /// </summary>
        [Optional]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public int MessageProcessorsCount { get; set; } = 8;

        /// <summary>
        /// Default first level retry timeout.
        /// </summary>
        [Optional]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public TimeSpan DefaultFirstLevelRetryTimeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Max age of the message to retry it at the first level (includes all attempts).
        /// </summary>
        [Optional]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public TimeSpan MaxFirstLevelRetryMessageAge { get; set; } = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Max count of the first level retries for a message.
        /// </summary>
        [Optional]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public int MaxFirstLevelRetryCount { get; set; } = 20;

        /// <summary>
        /// Max messages which can be retried at the first level retries at the moment.
        /// </summary>
        [Optional]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public int FirstLevelRetryQueueCapacity { get; set; } = 10000;

        /// <summary>
        /// Max message which can wait for the free processor right after the read by a consumer.
        /// </summary>
        [Optional]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public int ProcessingQueueCapacity { get; set; } = 5000;
    }
}
