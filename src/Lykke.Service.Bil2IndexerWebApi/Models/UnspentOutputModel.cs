using Newtonsoft.Json;

namespace DataApi.Models
{
    public class UnspentOutputLinks
    {
        public string TransactionUrl { get; set; }

    }
    public class UnspentOutputModel
    {
        public BalanceModel BalanceModel { get; set; }
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "_links")]
        public UnspentOutputLinks Links { get; set; }
    }
}
