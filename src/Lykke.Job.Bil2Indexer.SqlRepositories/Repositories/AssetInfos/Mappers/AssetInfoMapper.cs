using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.AssetInfos.Mappers
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
                Id = source.Asset.BuildId()
            };
        }

        public static AssetInfo ToDomain(this AssetInfoEntity source, string blockchainType)
        {
            return new AssetInfo(blockchainType, new Asset(source.AssetId, source.AssetAddress), source.Scale);
        }
    }
}
