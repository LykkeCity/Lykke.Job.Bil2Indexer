using System.Collections.Generic;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class BlockModelMapper
    {
        public static BlockModel Map(BlockHeader block)
        {
            throw new System.NotImplementedException();
        }

        public static Paginated<BlockModel> Map(IReadOnlyCollection<BlockHeader> blocks)
        {
            throw new System.NotImplementedException();
        }
    }
}
