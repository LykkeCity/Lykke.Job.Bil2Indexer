using System.ComponentModel.DataAnnotations.Schema;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models
{
    [Table("crawlers")]
    public class CrawlerEntity
    {
        [Column("stop_accembling_block")]
        public long StopAssemblingBlock { get; set; }
        
        public long Version { get; set; }

        [Column("start_block")]
        public long StartBlock { get; set; }
        
        [Column("sequence")]
        public long Sequence { get; set; }

        [Column("expected_block_number")]
        public long ExpectedBlockNumber { get; set; }
    }
}
