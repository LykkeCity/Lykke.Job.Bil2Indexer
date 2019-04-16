using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Job.Bil2Indexer.Domain;

namespace Lykke.Job.Bil2Indexer.VerifyingTool.BlockchainAdapters
{
    public interface IBlockchainVerifierAdapter
    {
        Task<(IEnumerable<TransferCoinsTransactionExecutedEvent> coinTransfers, IEnumerable<TransactionFailedEvent>
                failedTransfers)>
            GetTransactionsForBlockAsync(BigInteger blockNumber);

        Task<BlockHeader> GetBlockAsync(BigInteger blockNumber);
    }
}
