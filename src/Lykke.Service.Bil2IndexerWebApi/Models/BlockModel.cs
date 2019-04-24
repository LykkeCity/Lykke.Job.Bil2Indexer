using System;
using Newtonsoft.Json;

namespace DataApi.Models
{
    public class BlockLinks
    {
        public string TransactionUrl { get; set; }
        public string PrevBlockUrl { get; set; }
        public string RawUrl { get; set; }
    }

    public class BlockModel
    {
        public string Id { get; set; }
        
        public int Number { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public int Size { get; set; }

        public int TransactionsCount { get; set; }

        public string PrevBlockId { get; set; }

        [JsonProperty(PropertyName = "_links")]
        public BlockLinks Links { get; set; }
    }
}
