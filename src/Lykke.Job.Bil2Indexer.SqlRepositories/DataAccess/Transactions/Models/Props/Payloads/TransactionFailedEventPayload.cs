using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Bil2.Contract.Common;
using Lykke.Bil2.Contract.TransactionsExecutor;
using Newtonsoft.Json;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models.Props.Payloads
{
    public class TransactionFailedEventPayload
    {
        [CanBeNull]
        [JsonProperty("fees")]
        public IEnumerable<Fee> Fees { get; set; }

        [JsonProperty("errorCode")]
        public TransactionBroadcastingError ErrorCode { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }
    }
}
