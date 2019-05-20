﻿using System.ComponentModel.DataAnnotations.Schema;
using Lykke.Job.Bil2Indexer.Domain;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.IndexerState.Models
{
    [Table("chain_heads")]
    public class ChainHeadEntity
    {
        [Column("id")]
        public string Id { get; set; }

        [Column("first_block_number")]
        public long FirstBlockNumber { get; set; }

        [Column("block_number")]
        public long? BlockNumber { get; set; }

        [Column("block_id")]
        public string BlockId { get; set; }

        [Column("prev_block_id")]
        public string PreviousBlockId { get; set; }

        [Column("sequence")]
        public long Sequence { get; set; }

        [Column("crawler_sequence")]
        public long CrawlerSequence { get; set; }

        [Column("mode")]
        public ChainHeadMode Mode { get; set; }

        public uint Version { get; set; }
    }
}
