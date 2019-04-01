using System.Threading.Tasks;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Bil2.Contract.BlocksReader.Commands;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;

namespace Lykke.Job.Bil2Indexer.Workflow.BackgroundJobs
{
    public class RetryNotFoundBlockJob
    {
        private readonly IBlocksReaderApiFactory _blocksReaderApiFactory;
        private readonly ICrawlersManager _crawlersManager;
        private readonly ICrawlersRepository _crawlersRepository;

        public RetryNotFoundBlockJob(
            IBlocksReaderApiFactory blocksReaderApiFactory,
            ICrawlersManager crawlersManager,
            ICrawlersRepository crawlersRepository)
        {
            _blocksReaderApiFactory = blocksReaderApiFactory;
            _crawlersManager = crawlersManager;
            _crawlersRepository = crawlersRepository;
        }

        public async Task RetryAsync(string blockchainType, long blockNumber, CrawlerCorrelationId messageCorrelationId)
        {
            var crawler = await _crawlersManager.GetCrawlerAsync(blockchainType, blockNumber);
            var crawlerCorrelationId = crawler.GetCorrelationId();

            if (crawlerCorrelationId.IsPreviousOf(messageCorrelationId) ||
                crawlerCorrelationId.Equals(messageCorrelationId))
            {
                // Disordered job run, we should ignore it.
                return;
            }

            var blocksReaderApi = _blocksReaderApiFactory.Create(blockchainType);

            crawler.RetryCurrentBlock();

            await _crawlersRepository.SaveAsync(crawler);

            await blocksReaderApi.SendAsync(new ReadBlockCommand(blockNumber), crawler.GetCorrelationId().ToString());
        }
    }
}
