using DataApi.Core.Domain;
using DataApi.Models;
using DataApi.Models.Common;

namespace DataApi.Factories
{
    public interface IAssetModelFactory
    {
        AssetModel PrepareAssetModel(Asset asset);
        Paginated<AssetModel[]> PrepareAssetsPaginated(Asset[] assets);
    }
}
