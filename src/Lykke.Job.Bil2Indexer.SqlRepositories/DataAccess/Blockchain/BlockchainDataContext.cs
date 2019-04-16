using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Coins.Models;
using Microsoft.EntityFrameworkCore;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain
{
    public class BlockchainDataContext: DbContext
    {
        private readonly string _connectionString;

        public BlockchainDataContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<CoinEntity> Coins { get; set; }

        public DbSet<FeeEnvelopeEntity> FeeEnvelopes { get; set; }

        public DbSet<BalanceActionEntity> BalanceActions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connectionString);
        }
    }
}
