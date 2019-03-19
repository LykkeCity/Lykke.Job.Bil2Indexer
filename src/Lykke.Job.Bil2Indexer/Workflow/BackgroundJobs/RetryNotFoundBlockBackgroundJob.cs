using System.Threading.Tasks;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Bil2.Contract.BlocksReader.Commands;

namespace Lykke.Job.Bil2Indexer.Workflow.BackgroundJobs
{
    public class RetryNotFoundBlockBackgroundJob
    {
        private readonly IBlocksReaderApiFactory _blocksReaderApiFactory;

        public RetryNotFoundBlockBackgroundJob(IBlocksReaderApiFactory blocksReaderApiFactory)
        {
            _blocksReaderApiFactory = blocksReaderApiFactory;
        }

        public Task RetryAsync(string blockchainType, long blockNumber)
        {
            return _blocksReaderApiFactory.Create(blockchainType).SendAsync(new ReadBlockCommand(blockNumber));
        }
    }
}
