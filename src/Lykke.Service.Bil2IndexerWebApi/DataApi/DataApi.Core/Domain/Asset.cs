using System;

namespace DataApi.Core.Domain
{
    public class Asset
    {
        public string Ticker { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public int Accuracy { get; set; }
        public DateTime CreatedAt { get; set; }
        public Transaction Transaction { get; set; }
        public Block Block { get; set; }
    }
}
