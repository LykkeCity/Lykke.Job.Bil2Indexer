using Newtonsoft.Json;

namespace Lykke.Service.Bil2IndexerWebApi.Models
{
    public class TransactionModel
    {
        public string Id { get; set; }
        public string BlockId { get; set; }
        public long BlockNumber { get; set; }
        public int Number { get; set; }
        public TransferModel[] Transfers { get; set; }
        public FeeModel[] Fees { get; set; }
        public bool IsIrreversible { get; set; }
        public long ConfirmationsCount { get; set; }

        [JsonProperty(PropertyName = "_links")]
        public TransactionLinks Links { get; set; }
    }
}
