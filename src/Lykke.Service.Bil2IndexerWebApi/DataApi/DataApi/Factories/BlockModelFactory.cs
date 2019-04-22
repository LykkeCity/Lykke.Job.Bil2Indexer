using DataApi.Core.Domain;
using DataApi.Models;
using DataApi.Models.Common;

namespace DataApi.Factories
{
    public class BlockModelFactory : IBlockModelFactory
    {
        public BlockModel PrepareBlockModel(Block block)
        {
            throw new System.NotImplementedException();
        }

        public Paginated<BlockModel[]> PrepareBlocksPaginated(Block[] blocks)
        {
            throw new System.NotImplementedException();
        }
    }
}
