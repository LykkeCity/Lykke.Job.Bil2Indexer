using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;
using Newtonsoft.Json;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models.Props.Payloads
{
    public class TransferAmountTransactionExecutedPayload
    {
        [JsonProperty("balanceChanges")]
        public IEnumerable<BalanceChangeEntity> BalanceChanges { get; set; }

        [CanBeNull]
        [JsonProperty("fees")]
        public IEnumerable<Fee> Fees { get; set; }

        [JsonProperty("isIrreversible")]
        public bool? IsIrreversible { get; set; }
    }
}
