using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Factories
{
    public class AssetModelFactory : IAssetModelFactory
    {
        public AssetModel PrepareAssetModel(Asset asset)
        {
            throw new System.NotImplementedException();
        }

        public Paginated<AssetModel[]> PrepareAssetsPaginated(Asset[] assets)
        {
            throw new System.NotImplementedException();
        }
    }
}
