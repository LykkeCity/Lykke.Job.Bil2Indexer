using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain
{
    internal class DataContext: DbContext
    {
        private readonly string _connectionString;

        public DataContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<CoinEntity> Coins { get; set; }

        public DbSet<FeeEnvelopeEntity> FeeEnvelopes { get; set; }

        public DbSet<BalanceActionEntity> BalanceActions { get; set; }

        public DbSet<AssetInfoEntity> AssetInfos { get; set; }

        public DbSet<CrawlerEntity> Crawlers { get; set; }
        public DbSet<BlockHeaderEntity> BlockHeaders { get; set; }

        public DbSet<ChainHeadEntity> ChainHeads { get; set; }


        public DbSet<TransactionEntity> Transactions { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connectionString)
                .ConfigureWarnings(bulder => bulder.Throw(RelationalEventId.QueryClientEvaluationWarning));
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssetInfoEntity>()
                .HasKey(o => o.Id);
            modelBuilder.Entity<CrawlerEntity>()
                .HasKey(o => new { o.StartBlock, o.StopAssemblingBlock });

            modelBuilder.Entity<CrawlerEntity>()
                .Property(p => p.Version)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

            modelBuilder.Entity<BlockHeaderEntity>()
                .HasKey(o => o.Id);

            modelBuilder.Entity<BlockHeaderEntity>()
                .Property(p => p.Version)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();

            modelBuilder.Entity<ChainHeadEntity>()
                .HasKey(o => o.Id);

            modelBuilder.Entity<ChainHeadEntity>()
                .Property(p => p.Version)
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .ValueGeneratedOnAddOrUpdate()
                .IsConcurrencyToken();
        }
    }
}
