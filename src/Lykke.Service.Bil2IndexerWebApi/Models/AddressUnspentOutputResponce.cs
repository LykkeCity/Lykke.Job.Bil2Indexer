namespace Lykke.Service.Bil2IndexerWebApi.Models
{
    public class AddressUnspentOutputResponce
    {
        public string Id { get; set; }
        public AddressBalanceChangeResponce AddressBalanceChangeResponce { get; set; }
        public string TransactionId { get; set; }
    }
}
