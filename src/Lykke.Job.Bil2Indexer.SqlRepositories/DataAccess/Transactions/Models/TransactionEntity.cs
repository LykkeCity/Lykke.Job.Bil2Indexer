using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models
{
    [Table("transactions")]
    public class TransactionEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("blockchain_type")]
        public string BlockchainType { get; set; }

        [Column("block_id")]
        public string BlockId { get; set; }

        [Column("transaction_id")]
        public string TransactionId { get; set; }

        [Column("transaction_number")]
        public int TransactionNumber { get; set; }

        [Column("type")]
        public TransactionType Type { get; set; }

        [Column("payload", TypeName = "jsonb")]
        public string Payload { get; set; }
    }
}
