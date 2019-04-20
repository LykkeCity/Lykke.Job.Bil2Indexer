using System;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models
{
    [Table("balance_actions")]
    public class BalanceActionEntity
    {
        [Column("id")]
        public Guid Id { get; set; }
        
        [Column("blockchain_type")]
        public string BlockchainType { get; set; }

        [Column("block_id")]
        public string BlockId { get; set; }

        [Column("block_number")]
        public long BlockNumber { get; set; }

        [Column("asset_id")]
        public string AssetId { get; set; }

        [Column("asset_address")]
        [CanBeNull]
        public string AssetAddress { get; set; }

        [Column("transaction_id")]
        public string TransactionId { get; set; }

        [Column("value")]
        public decimal Value { get; set; }

        [Column("value_scale")]
        public int ValueScale { get; set; }

        [Column("value_string")]
        public string ValueString { get; set; }
        
        [NotMapped]
        public Money ValueMoney { get; set; }

        [Column("address")]
        public string Address { get; set; }
    }
}
