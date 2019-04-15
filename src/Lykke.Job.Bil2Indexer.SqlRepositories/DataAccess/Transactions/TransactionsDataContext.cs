using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions
{
    public class TransactionsDataContext : DbContext
    {
        private readonly string _connectionString;

        public TransactionsDataContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<TransactionEntity> Transactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TransactionEntity>()
                .HasKey(o => o.Id);
        }
    }
}
