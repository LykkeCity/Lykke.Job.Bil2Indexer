using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models
{
    [Table("coins")]
    public class CoinEntity
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("transaction_id")]
        public string TransactionId { get; set; }
        
        [Column("coin_id")]
        public string CoinId { get; set; }

        [Column("coin_number")]
        public int CoinNumber { get; set; }

        [Column("asset_id")]
        public string AssetId { get; set; }

        [CanBeNull]
        [Column("asset_address")]
        public string AssetAddress { get; set; }
        
        [Column("value_scale")]
        public int ValueScale { get; set; }

        [Column("value_string")]
        public string ValueString { get; set; }

        [CanBeNull]
        [Column("address")]
        public string Address { get; set; }

        [CanBeNull]
        [Column("address_tag")]
        public string AddressTag { get; set; }

        [CanBeNull]
        [Column("address_tag_type")]
        public AddressTagTypeValues? AddressTagType { get; set; }
        public enum AddressTagTypeValues
        {
            Number = 0,
            Text = 1
        }

        [Column("address_nonce")]
        public long? AddressNonce { get; set; }

        [Column("is_spent")]
        public bool IsSpent { get; set; }
    }
}
