using Newtonsoft.Json;

namespace DataApi.Models
{
    public class TransactionLinks
    {
        public string RawUrl { get; set; }
        public string BlockUrl { get; set; }
    }

    public class TransactionModel
    {
        public string Id { get; set; }
        public string BlockId { get; set; }
        public int Number { get; set; }

        public TransferModel[] Transfers { get; set; }

        [JsonProperty(PropertyName = "_links")]
        public TransactionLinks Links { get; set; }
    }
}
