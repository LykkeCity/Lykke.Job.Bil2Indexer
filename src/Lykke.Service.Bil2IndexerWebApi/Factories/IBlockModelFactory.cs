using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Factories
{
    public interface IBlockModelFactory
    {
        BlockModel PrepareBlockModel(BlockHeader block);
        Paginated<BlockModel[]> PrepareBlocksPaginated(BlockHeader[] blocks);
    }
}
