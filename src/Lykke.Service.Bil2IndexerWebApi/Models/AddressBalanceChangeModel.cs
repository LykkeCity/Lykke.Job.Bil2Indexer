﻿namespace Lykke.Service.Bil2IndexerWebApi.Models
{
    public class AddressBalanceChangeModel
    {
        public string Address { get; set; }
        public AssetIdModel AssetId { get; set; }
        public string Amount { get; set; }
        public long BlockNumber { get; set; }
        public string BlockId { get; set; }
        public bool IsIrreversible { get; set; }
        public long ConfirmationsCount { get; set; }
    }
}
