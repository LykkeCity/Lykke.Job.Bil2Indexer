using System.Collections.Generic;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class AssetModelMapper
    {
        public static ActionResult<Paginated<AssetModel[]>> Map(IReadOnlyCollection<AssetInfo> assets)
        {
            throw new System.NotImplementedException();
        }
    }
}
