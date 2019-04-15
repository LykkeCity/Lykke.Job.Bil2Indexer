using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Coins.Models
{
    [Table("coins")]
    public class CoinEntity
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("blockchain_type")]
        public string BlockchainType { get; set; }
    }
}
