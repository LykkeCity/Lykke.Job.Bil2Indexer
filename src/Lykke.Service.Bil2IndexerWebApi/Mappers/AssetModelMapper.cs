using System.Collections.Generic;
using System.Linq;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class AssetModelMapper
    {
        public static IReadOnlyCollection<AssetModel> ToViewModel(this IReadOnlyCollection<AssetInfo> source)
        {
            return source.Select(ToViewModel).ToList();
        }

        public static AssetModel ToViewModel(this AssetInfo source)
        {
            return new AssetModel
            {
                Accuracy = source.Scale,
                Id = new AssetIdModel
                {
                    Address = source.Asset.Address,
                    Ticker = source.Asset.Id
                }
            };
        }
    }
}
