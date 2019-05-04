using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models;
using NpgsqlTypes;
using PostgreSQLCopyHelper;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions.Mappers
{
    internal static class TransactionCopyMapper
    {
        public static PostgreSQLCopyHelper<TransactionEntity> BuildCopyMapper()
        {
            return new PostgreSQLCopyHelper<TransactionEntity>("transactions")
                .UsePostgresQuoting()
                .Map("blockchain_type", p => p.BlockchainType, NpgsqlDbType.Varchar)
                .Map("block_id", p => p.BlockId, NpgsqlDbType.Varchar)
                .Map("transaction_id", p => p.TransactionId, NpgsqlDbType.Varchar)
                .Map("transaction_number", p => p.TransactionNumber, NpgsqlDbType.Integer)
                .Map("type", p => p.Type, NpgsqlDbType.Integer)
                .Map("payload", p => p.Payload, NpgsqlDbType.Jsonb);
        }
    }
}
