using System.Collections.Generic;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class AssetModelMapper
    {
        public static IReadOnlyCollection<AssetModel> ToViewModel(this IReadOnlyCollection<AssetInfo> assets)
        {
            throw new System.NotImplementedException();
        }

        public static AssetModel ToViewModel(this AssetInfo source)
        {
            throw new System.NotImplementedException();
        }

        public static IReadOnlyCollection<AssetModel> WrapPaginated(this AssetInfo source, PaginationModel model)
        {
            throw new System.NotImplementedException();
        }
    }
}
