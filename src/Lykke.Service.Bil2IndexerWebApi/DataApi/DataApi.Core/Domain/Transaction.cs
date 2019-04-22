namespace DataApi.Core.Domain
{
    public class Transaction
    {
        public string Id { get; set; }
        public Block Block { get; set; }

        public Transfer[] Transfers { get; set; }
    }
}
