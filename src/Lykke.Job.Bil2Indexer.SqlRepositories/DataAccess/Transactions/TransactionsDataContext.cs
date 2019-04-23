using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions
{
    internal class TransactionsDataContext : DbContext
    {
        private readonly string _connectionString;

        public TransactionsDataContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<TransactionEntity> Transactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connectionString)
                .ConfigureWarnings(bulder => bulder.Throw(RelationalEventId.QueryClientEvaluationWarning));
        }
    }
}
