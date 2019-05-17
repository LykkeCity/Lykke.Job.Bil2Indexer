using System.Collections.Generic;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class BlockModelMapper
    {
        public static BlockModel ToViewModel(this BlockHeader block)
        {
            throw new System.NotImplementedException();
        }

        public static IReadOnlyCollection<BlockModel> ToViewModel(this IReadOnlyCollection<BlockHeader> blocks)
        {
            throw new System.NotImplementedException();
        }
    }
}
