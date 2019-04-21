using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;
using NpgsqlTypes;
using PostgreSQLCopyHelper;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.FeeEnvelopes.Mappers
{
    internal static class FeeCopyMapper
    {
        public static PostgreSQLCopyHelper<FeeEnvelopeEntity> BuildCopyMapper()
        {
            return new PostgreSQLCopyHelper<FeeEnvelopeEntity>("fees")
                .UsePostgresQuoting()
                .Map("blockchain_type", p => p.BlockchainType, NpgsqlDbType.Varchar)
                .Map("block_id", p => p.BlockId, NpgsqlDbType.Varchar)
                .Map("transaction_id", p => p.TransactionId, NpgsqlDbType.Varchar)
                .Map("asset_id", p => p.AssetId, NpgsqlDbType.Varchar)
                .Map("asset_address", p => p.AssetAddress, NpgsqlDbType.Varchar)
                .Map("value_string", p => p.ValueString, NpgsqlDbType.Varchar)
                .Map("value_scale", p => p.ValueScale, NpgsqlDbType.Integer);
        }
    }
}
