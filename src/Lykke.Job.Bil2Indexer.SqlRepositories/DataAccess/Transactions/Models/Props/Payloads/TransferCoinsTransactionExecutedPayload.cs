using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.Contract.Common;
using Newtonsoft.Json;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models.Props.Payloads
{
    public class TransferCoinsTransactionExecutedPayload
    {
        [JsonProperty("receivedCoins")]
        public IEnumerable<ReceivedCoin> ReceivedCoins { get; set; }

        [JsonProperty("spentCoins")]
        public IEnumerable<CoinReference> SpentCoins { get; set; }

        [CanBeNull]
        [JsonProperty("fees")]
        public IEnumerable<Fee> Fees { get; set; }

        [CanBeNull]
        [JsonProperty("isIrreversible")]
        public bool? IsIrreversible { get; set; }
    }
}
