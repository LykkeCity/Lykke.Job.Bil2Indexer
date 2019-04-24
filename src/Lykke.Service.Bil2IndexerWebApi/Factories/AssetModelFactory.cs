using DataApi.Core.Domain;
using DataApi.Models;
using DataApi.Models.Common;

namespace DataApi.Factories
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
