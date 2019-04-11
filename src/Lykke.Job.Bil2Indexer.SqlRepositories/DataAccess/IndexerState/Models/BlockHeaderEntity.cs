using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.IndexerState.Models
{
    [Table("block_headers")]
    public class BlockHeaderEntity
    {
        [Column("blockchain_type")]
        public string BlockchainType { get; set; }

        [Column("number")]
        public long Number { get; set; }

        [Column("mined_at")]
        public DateTime MinedAt { get; set; }

        [Column("size")]
        public int Size { get; set; }

        [Column("transaction_count")]
        public int TransactionCount { get; set; }
        
        [Column("previous_block_id")]
        public string PreviousBlockId { get; set; }

        [Column("id")]
        public string Id { get; set; }

        public uint Version { get; set; }

        [Column("state")]
        public BlockState State { get; set; }

        public enum BlockState
        {
            Assembling = 0,
            Assembled = 1,
            Executed = 2,
            PartiallyExecuted = 3,
            RolledBack = 4
        }
    }
}
