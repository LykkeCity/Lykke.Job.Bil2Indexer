using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.FeeEnvelopes.Mappers
{
    internal static class FeeDbEntityMapper
    {
        public static FeeEnvelope ToDomain(this FeeEnvelopeEntity source, string blockchainType)
        {
            return new FeeEnvelope(blockchainType,
                source.BlockId,
                source.TransactionId,
                new Fee(new Asset(source.AssetId, source.AssetAddress != null ? new AssetAddress(source.AssetAddress) : null),
                    MoneyHelper.BuildUMoney(source.ValueString,
                        source.ValueScale)));
        }


        public static FeeEnvelopeEntity ToDbEntity(this FeeEnvelope source)
        {
            return new FeeEnvelopeEntity
            {
                TransactionId = source.TransactionId,
                AssetAddress = source.Fee.Asset.Address,
                AssetId = source.Fee.Asset.Id,
                BlockId = source.BlockId,
                ValueScale = source.Fee.Amount.Scale,
                ValueString = source.Fee.Amount.ToString()
            };
        }
    }
}
