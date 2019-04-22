namespace DataApi.Core.Domain
{
    public class Balance
    {
        public Asset Asset { get; set; }
        public decimal Amount { get; set; }
        public Block Block { get; set; }
    }
}
