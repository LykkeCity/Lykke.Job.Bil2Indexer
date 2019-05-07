using System.Data.Common;
using System.Diagnostics;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.IndexerState.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.IndexerState
{
    public class StateDataContext : DbContext
    {
        private readonly string _connectionString;

        public StateDataContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<CrawlerEntity> Crawlers { get; set; }
        public DbSet<BlockHeaderEntity> BlockHeaders { get; set; }
        public DbSet<ChainHeadEntity> ChainHeads { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connectionString)
                .ConfigureWarnings(bulder => bulder.Throw(RelationalEventId.QueryClientEvaluationWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CrawlerEntity>()
                .HasKey(o => new {o.StartBlock, o.StopAssemblingBlock });
            
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
