namespace Lykke.Service.Bil2IndexerWebApi.Models
{
    public class AddressUnspentOutputModel
    {
        public string Id { get; set; }
        public AddressBalanceChangeModel AddressBalanceChangeModel { get; set; }
        public string TransactionId { get; set; }
    }
}
