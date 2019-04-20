using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;
using NpgsqlTypes;
using PostgreSQLCopyHelper;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.BalanceActions.Mappers
{
    public static class BalanceActionCopyMapper
    {
        public static PostgreSQLCopyHelper<BalanceActionEntity> BuildCopyMapper()
        {
            return new PostgreSQLCopyHelper<BalanceActionEntity>("balance_actions")
                .UsePostgresQuoting()
                .Map("blockchain_type", p => p.BlockchainType, NpgsqlDbType.Varchar)
                .Map("block_id", p => p.BlockId, NpgsqlDbType.Varchar)
                .Map("block_number", p => p.BlockNumber, NpgsqlDbType.Integer)
                .Map("asset_id", p => p.AssetId, NpgsqlDbType.Varchar)
                .Map("asset_address", p => p.AssetAddress, NpgsqlDbType.Varchar)
                .Map("transaction_id", p => p.TransactionId, NpgsqlDbType.Varchar)
                .Map("value_string", p => p.ValueString, NpgsqlDbType.Varchar)
                .Map("value_scale", p => p.ValueScale, NpgsqlDbType.Integer)
                .Map("address", p => p.Address, NpgsqlDbType.Varchar);
        }
    }
}
