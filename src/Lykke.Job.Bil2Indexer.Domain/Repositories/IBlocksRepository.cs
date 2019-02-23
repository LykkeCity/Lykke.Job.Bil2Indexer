﻿using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface IBlocksRepository
    {
        Task SaveAsync(BlockHeader block);
        Task<BlockHeader> GetOrDefaultAsync(long blockNumber);
        //Task<BlockHeader> GetLastValidOrDefault();
        Task RemoveAsync(BlockHeader block);
    }
}
