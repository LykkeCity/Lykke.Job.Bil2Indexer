using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DataApi.Models.Common
{
    public enum PaginationOrder
    {
        Asc,
        Desc
    }

    public class PaginationModel
    {
        public string EndingBefore { get; set; }

        public string StartingAfter { get; set; }

        public int Limit { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PaginationModel Order { get; set; }

        public string PrevUrl { get; set; }

        public string NextUrl { get; set; }
    }
}
