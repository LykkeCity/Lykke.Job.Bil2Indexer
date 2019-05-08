using Newtonsoft.Json;

namespace Lykke.Service.Bil2IndexerWebApi.Models
{
    public class AddressUnspentOutputModel
    {
        public AddressBalanceModel AddressBalanceModel { get; set; }
        public string TransactionId { get; set; }
    }
}
