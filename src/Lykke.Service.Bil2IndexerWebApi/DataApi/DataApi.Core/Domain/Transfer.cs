namespace DataApi.Core.Domain
{
    public class Transfer
    {
        public string Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; }
        public Asset Asset { get; set; }
    }
}
