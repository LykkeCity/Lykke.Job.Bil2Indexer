using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;

namespace Lykke.Job.Bil2Indexer.VerifyingTool.BlockchainAdapters
{
    public interface IBlockchainVerifierAdapter
    {
        Task<IReadOnlyCollection<Transaction>> GetBlockTransactionsAsync(BigInteger blockNumber);

        Task<BlockHeader> GetBlockAsync(BigInteger blockNumber);
    }
}
