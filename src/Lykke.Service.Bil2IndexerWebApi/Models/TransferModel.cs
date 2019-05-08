namespace Lykke.Service.Bil2IndexerWebApi.Models
{
    public class TransferModel
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Amount { get; set; }
        public AssetIdModel AssetId { get; set; }
        public string TransferId { get; set; }
    }
}
