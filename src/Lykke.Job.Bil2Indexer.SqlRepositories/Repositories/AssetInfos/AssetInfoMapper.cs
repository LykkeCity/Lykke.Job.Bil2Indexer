using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.AssetInfos
{
    public static class AssetInfoMapper
    {
        public static AssetInfoEntity ToDbEntity(this AssetInfo source)
        {
            return new AssetInfoEntity
            {
                BlockchainType = source.BlockchainType,
                AssetId = source.Asset.Id,
                Scale = source.Scale,
                AssetAddress = source.Asset.Address,
                Id = BuildId(source.Asset)
            };
        }

        public static AssetInfo ToDomain(this AssetInfoEntity source)
        {
            return new AssetInfo(source.BlockchainType, new Asset(source.AssetId, source.AssetAddress), source.Scale);
        }

        public static string BuildId(Asset asset)
        {
            if (asset.Address?.Value == null)
            {
                return asset.Id;
            }

            return $"{asset.Id}_{asset.Address}";
        }
    }
}
