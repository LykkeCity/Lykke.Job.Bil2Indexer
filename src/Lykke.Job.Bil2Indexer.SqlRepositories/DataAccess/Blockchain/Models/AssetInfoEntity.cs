using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models
{
    [Table("assets")]
    public class AssetInfoEntity
    {
        [Column("id")]
        public string Id { get; set; }

        [Column("blockchain_type")]
        public string BlockchainType { get; set; }

        [Column("asset_id")]
        public string AssetId { get; set; }

        [Column("asset_address")]
        public string AssetAddress { get; set; }

        [Column("scale")]
        public int Scale { get; set; }
    }
}
