using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Service.Bil2IndexerWebApi.Models.Common
{
    public class PaginationModel<T>
    {
        public T EndingBefore { get; set; }

        public T StartingAfter { get; set; }

        public int Count { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PaginationOrder Order { get; set; }

        public string PrevUrl { get; set; }

        public string NextUrl { get; set; }
    }
}
