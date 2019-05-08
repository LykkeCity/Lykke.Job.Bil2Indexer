using Newtonsoft.Json;

namespace Lykke.Service.Bil2IndexerWebApi.Models
{
    public class BlockchainModel
    {
        public string BlockchainType { get; set; }

        [JsonProperty(PropertyName = "_links")]
        public BlockchainLinksModel Links { get; set; }
    }
}
