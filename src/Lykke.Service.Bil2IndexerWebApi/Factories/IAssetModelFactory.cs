using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Factories
{
    public interface IAssetModelFactory
    {
        AssetModel PrepareAssetModel(Asset asset);
        Paginated<AssetModel[]> PrepareAssetsPaginated(Asset[] assets);
    }
}
