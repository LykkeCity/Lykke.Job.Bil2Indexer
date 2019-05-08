using System;
using Newtonsoft.Json;

namespace Lykke.Service.Bil2IndexerWebApi.Models
{
    public class BlockModel
    {
        public string Id { get; set; }
        public int Number { get; set; }
        public DateTime MinedAt { get; set; }
        public int Size { get; set; }
        public int TransactionsCount { get; set; }
        public string PrevBlockId { get; set; }
        public bool IsIrreversible { get; set; }
        public long ConfirmationsCount { get; set; }

        [JsonProperty(PropertyName = "_links")]
        public BlockLinks Links { get; set; }
    }
}
