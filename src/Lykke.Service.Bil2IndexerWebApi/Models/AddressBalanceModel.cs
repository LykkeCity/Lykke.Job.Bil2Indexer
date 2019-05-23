namespace Lykke.Service.Bil2IndexerWebApi.Models
{
    public class AddressBalanceModel
    {
        public string Address { get; set; }
        public AssetIdModel AssetId { get; set; }
        public string Amount { get; set; }

        // by pass
        public string BlockId { get; set; }

        // by pass
        public long? BlockNumber { get; set; }
    }
}
