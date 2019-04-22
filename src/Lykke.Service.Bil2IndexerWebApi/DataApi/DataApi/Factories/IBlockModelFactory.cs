using DataApi.Core.Domain;
using DataApi.Models;
using DataApi.Models.Common;

namespace DataApi.Factories
{
    public interface IBlockModelFactory
    {
        BlockModel PrepareBlockModel(Block block);
        Paginated<BlockModel[]> PrepareBlocksPaginated(Block[] blocks);
    }
}
