using Newtonsoft.Json;

namespace Lykke.Service.Bil2IndexerWebApi.Models
{
    public class BlockchainResponce
    {
        public string BlockchainType { get; set; }

        [JsonProperty(PropertyName = "_links")]
        public BlockchainLinksResponce Links { get; set; }
    }
}
