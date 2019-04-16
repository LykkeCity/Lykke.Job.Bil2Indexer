using System.ComponentModel.DataAnnotations.Schema;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models
{
    [Table("assets")]
    public class AssetInfoEntity
    {
        [Column("blockchain_type")]
        public string BlockchainType { get; set; }

        [Column("id")]
        public string Id { get; set; }

        [Column("scale")]
        public int Scale { get; set; }
    }
}
