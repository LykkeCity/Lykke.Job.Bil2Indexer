using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Factories
{
    public class BlockModelFactory : IBlockModelFactory
    {
        public BlockModel PrepareBlockModel(BlockHeader block)
        {
            throw new System.NotImplementedException();
        }

        public Paginated<BlockModel[]> PrepareBlocksPaginated(BlockHeader[] blocks)
        {
            throw new System.NotImplementedException();
        }
    }
}
