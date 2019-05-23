using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;
using Newtonsoft.Json;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models.Props.Payloads
{
    internal class FailedTransactionPayload
    {
        [CanBeNull]
        [JsonProperty("fees")]
        public IEnumerable<Fee> Fees { get; set; }

        [JsonProperty("errorCode")]
        public TransactionBroadcastingErrorEntity ErrorCode { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }
    }
}
