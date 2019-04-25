using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;
using NpgsqlTypes;
using PostgreSQLCopyHelper;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Coins.Mappers
{
    internal static class CoinCopyMapper
    {
        public static PostgreSQLCopyHelper<CoinEntity> BuildCopyMapper()
        {
            return new PostgreSQLCopyHelper<CoinEntity>("coins")
                .UsePostgresQuoting()
                .Map("blockchain_type", p => p.BlockchainType, NpgsqlDbType.Varchar)
                .Map("transaction_id", p => p.TransactionId, NpgsqlDbType.Varchar)
                .Map("coin_number", p => p.CoinNumber, NpgsqlDbType.Integer)
                .Map("asset_id", p => p.AssetId, NpgsqlDbType.Varchar)
                .Map("asset_address", p => p.AssetAddress, NpgsqlDbType.Varchar)
                .Map("value_string", p => p.ValueString, NpgsqlDbType.Varchar)
                .Map("value_scale", p => p.ValueScale, NpgsqlDbType.Integer)
                .Map("address", p => p.Address, NpgsqlDbType.Varchar)
                .Map("address_tag", p => p.AddressTag, NpgsqlDbType.Varchar)
                .Map("is_deleted", p => p.IsDeleted, NpgsqlDbType.Boolean)
                .MapNullable("address_tag_type", p => (int?) p.AddressTagType, NpgsqlDbType.Smallint)
                .Map("address_nonce", p => p.AddressNonce, NpgsqlDbType.Numeric)
                .Map("is_spent", p => p.IsSpent, NpgsqlDbType.Boolean)
                .Map("coin_id", p => p.CoinId, NpgsqlDbType.Varchar);
        }
    }
}
