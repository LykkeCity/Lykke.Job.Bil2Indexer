namespace DataApi.Models
{
    public class TransferModel
    {
        public string From { get; set; }
        public string To { get; set; }
        public decimal Amount { get; set; }
        public string Asset { get; set; }
        public string TransferId { get; set; }
    }
}
