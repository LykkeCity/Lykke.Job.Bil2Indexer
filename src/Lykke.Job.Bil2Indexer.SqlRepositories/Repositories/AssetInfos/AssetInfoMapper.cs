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
                AssetId = source.Asset.Id,
                Scale = source.Scale,
                AssetAddress = source.Asset.Address,
                Id = BuildId(source.Asset)
            };
        }

        public static AssetInfo ToDomain(this AssetInfoEntity source, string blockchainType)
        {
            return new AssetInfo(blockchainType, new Asset(source.AssetId, source.AssetAddress), source.Scale);
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
