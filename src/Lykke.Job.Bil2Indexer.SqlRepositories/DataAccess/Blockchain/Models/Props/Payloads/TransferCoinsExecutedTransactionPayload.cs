using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;
using Newtonsoft.Json;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models.Props.Payloads
{
    internal class TransferCoinsExecutedTransactionPayload
    {
        [JsonProperty("receivedCoins")]
        public IEnumerable<ReceivedCoinEntity> ReceivedCoins { get; set; }

        [JsonProperty("spentCoins")]
        public IEnumerable<CoinId> SpentCoins { get; set; }

        [CanBeNull]
        [JsonProperty("fees")]
        public IEnumerable<Fee> Fees { get; set; }

        [CanBeNull]
        [JsonProperty("isIrreversible")]
        public bool? IsIrreversible { get; set; }
    }
}
