using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models
{
    [Table("fees")]
    internal class FeeEnvelopeEntity
    {
        [Column("id")]
        [Key]
        public Guid Id { get; set; }

        [Column("block_id")]
        public string BlockId { get; set; }

        [Column("transaction_id")]
        public string TransactionId { get; set; }

        [Column("asset_id")]
        public string AssetId { get; set; }

        [Column("asset_address")]
        [CanBeNull]
        public string AssetAddress { get; set; }

        [Column("value_string")]
        public string ValueString { get; set; }

        [Column("value_scale")]
        public int ValueScale { get; set; }
    }
}
