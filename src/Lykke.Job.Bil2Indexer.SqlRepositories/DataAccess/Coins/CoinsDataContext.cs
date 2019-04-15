using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Coins.Models;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Coins
{
    public class CoinsDataContext: DbContext
    {
        private readonly string _connectionString;

        public CoinsDataContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<CoinEntity> Coins { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connectionString);
        }
    }
}
