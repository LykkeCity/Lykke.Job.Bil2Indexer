namespace Lykke.Service.Bil2IndexerWebApi.Models
{
    public class AddressBalanceModel
    {
        public string Address { get; set; }
        public AssetIdModel AssetId { get; set; }
        public string Amount { get; set; }

        //TODO add block num/id irreversible factor
    }
}
