namespace Lykke.Service.Bil2IndexerWebApi.Models
{
    public class TransferResponce
    {
        public string Address { get; set; }
        public string Amount { get; set; }
        public AssetIdResponce AssetId { get; set; }
        public string TransferId { get; set; }
    }
}
