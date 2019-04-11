using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.IndexerState.Models
{
    [Table("chain_heads")]
    public class ChainHeadEntity
    {
        [Column("blockchain_type")]
        public string BlockchainType { get; set; }

        [Column("first_block_number")]
        public long FirstBlockNumber { get; set; }

        [Column("block_number")]
        public long? BlockNumber { get; set; }

        [Column("block_id")]
        public string BlockId { get; set; }

        public uint Version { get; set; }
    }
}
