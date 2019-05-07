using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations
{
    [UsedImplicitly]
    public class BlockchainIndexerSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public IReadOnlyList<ChainCrawlerSettings> ChainCrawlers { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public TimeSpan NotFoundBlockRetryDelay { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public long BlockNumberToStartTransactionEventsPublication { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string PgBlockchainDataConnString { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string PgStateDataConnString { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string PgTransactionsDataConnString { get; set; }
    }
}
