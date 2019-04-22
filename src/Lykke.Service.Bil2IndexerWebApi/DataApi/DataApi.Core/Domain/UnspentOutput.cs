namespace DataApi.Core.Domain
{
    public class UnspentOutput
    {
        public Balance Balance { get; set; }
        public Transaction Transaction { get; set; }
    }
}
